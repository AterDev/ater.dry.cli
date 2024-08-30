using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Share.Models;


namespace Share.Infrastructure.Helper;
/// <summary>
/// openapi 解析帮助类
/// </summary>
public class OpenApiHelper
{
    public OpenApiDocument OpenApi { get; set; }
    /// <summary>
    /// 接口信息
    /// </summary>
    public List<RestApiGroup> RestApiGroups { get; set; }
    /// <summary>
    /// 所有请求及返回类型信息
    /// </summary>
    public List<EntityInfo> ModelInfos { get; set; }
    /// <summary>
    /// tag信息
    /// </summary>
    public List<ApiDocTag> OpenApiTags { get; set; }

    public OpenApiHelper(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        OpenApiTags = openApi.Tags
            .Select(s => new ApiDocTag
            {
                Name = s.Name,
                Description = s.Description
            })
            .ToList();
        ModelInfos = GetEntityInfos();
        RestApiGroups = GetRestApiGroups();
    }

    /// <summary>
    /// 接口信息
    /// </summary>
    /// <returns></returns>
    public List<RestApiGroup> GetRestApiGroups()
    {
        var apiInfos = new List<RestApiInfo>();
        foreach (KeyValuePair<string, OpenApiPathItem> path in OpenApi.Paths)
        {
            foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
            {
                RestApiInfo apiInfo = new()
                {
                    Summary = operation.Value.Summary,
                    OperationType = operation.Key,
                    OperationId = operation.Value.OperationId,
                    Router = path.Key,
                    Tag = operation.Value.Tags.FirstOrDefault()?.Name,
                };

                // 处理请求内容
                var requestBody = operation.Value.RequestBody;
                var requestParameters = operation.Value.Parameters;
                var responseBody = operation.Value.Responses;

                // 请求类型
                if (requestBody != null)
                {
                    var (RequestType, RequestRefType) = GetParamType(requestBody.Content.Values.FirstOrDefault()?.Schema);
                    // 关联的类型
                    var model = ModelInfos.FirstOrDefault(m => m.Name == RequestRefType);

                    if (model == null)
                    {
                        apiInfo.RequestInfo = new EntityInfo
                        {
                            Name = RequestType,
                            ProjectId = Const.PROJECT_ID,
                        };

                        if (!string.IsNullOrWhiteSpace(RequestType))
                        {
                            apiInfo.RequestInfo.PropertyInfos = new List<PropertyInfo>
                            {
                                new PropertyInfo
                                {
                                    Name = RequestType,
                                    Type = RequestRefType ?? RequestType,
                                }
                            };
                        }
                    }
                    else
                    {
                        apiInfo.RequestInfo = model;
                    }
                }
                // 响应类型
                if (responseBody != null)
                {
                    var (ResponseType, ResponseRefType) = GetParamType(responseBody
                       .FirstOrDefault().Value?.Content
                       .FirstOrDefault().Value?.Schema);
                    // 关联的类型
                    var model = ModelInfos.FirstOrDefault(m => m.Name == ResponseRefType);

                    // 返回内容没有对应类型
                    if (model == null)
                    {
                        apiInfo.ResponseInfo = new EntityInfo
                        {
                            Name = ResponseType,
                            ProjectId = Const.PROJECT_ID,
                        };
                        if (!string.IsNullOrWhiteSpace(ResponseType))
                        {
                            apiInfo.ResponseInfo.PropertyInfos = new List<PropertyInfo>
                            {
                                new PropertyInfo
                                {
                                    Name = ResponseType,
                                    Type = ResponseRefType ?? ResponseType,
                                }
                            };
                        }
                    }
                    else
                    {
                        apiInfo.ResponseInfo = model;
                    }
                    if (ResponseType.EndsWith("[]")) { apiInfo.ResponseInfo.IsList = true; }
                }
                // 请求的参数
                if (requestParameters != null)
                {
                    var parammeters = requestParameters?.Select(p =>
                    {
                        string? location = p.In?.GetDisplayName();
                        bool? inpath = location?.ToLower()?.Equals("path");
                        (string type, string? _) = GetParamType(p.Schema);
                        return new PropertyInfo
                        {
                            CommentSummary = p.Description,
                            Name = p.Name,
                            IsRequired = p.Required,
                            Type = type
                        };
                    }).ToList();
                    apiInfo.QueryParameters = parammeters;
                }
                apiInfos.Add(apiInfo);
            }
        }
        var apiGroups = new List<RestApiGroup>();
        OpenApiTags.ForEach(tag =>
        {
            var group = new RestApiGroup
            {
                Name = tag.Name,
                Description = tag.Description,
                ApiInfos = apiInfos.Where(a => a.Tag == tag.Name).ToList()
            };

            apiGroups.Add(group);
        });
        // tag不在OpenApiTags中的api infos
        var tags = OpenApiTags.Select(t => t.Name).ToList();
        var noTagApisInfo = apiInfos.Where(a => !tags.Contains(a.Tag)).ToList();

        if (noTagApisInfo.Any())
        {
            var group = new RestApiGroup
            {
                Name = "No Tags",
                Description = "无tag分组接口",
                ApiInfos = noTagApisInfo
            };
            apiGroups.Add(group);
        }
        return apiGroups;
    }

    /// <summary>
    /// 解析模型
    /// </summary>
    /// <returns></returns>
    public List<EntityInfo> GetEntityInfos()
    {
        var models = new List<EntityInfo>();

        foreach (KeyValuePair<string, OpenApiSchema> schema in OpenApi.Components.Schemas)
        {
            string name = schema.Key;
            string description = schema.Value.AllOf.LastOrDefault()?.Description
                ?? schema.Value.Description;
            description = description?.Replace("\n", " ") ?? "";
            List<PropertyInfo> props = ParseProperties(schema.Value);

            var model = new EntityInfo
            {
                Name = name,
                ProjectId = Const.PROJECT_ID,
                PropertyInfos = props,
                Comment = description,
            };
            // 判断是否为枚举类
            var enumNode = schema.Value.Enum;
            if (enumNode.Any())
            {
                model.IsEnum = true;
                model.PropertyInfos = GetEnumProperties(schema.Value);
            }
            models.Add(model);
        }
        return models;
    }

    /// <summary>
    /// 解析枚举类属性
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static List<PropertyInfo> GetEnumProperties(OpenApiSchema schema)
    {
        var props = new List<PropertyInfo>();
        var enums = schema.Enum.ToList();

        var extEnum = schema.Extensions.Where(e => e.Key == "x-enumNames").FirstOrDefault();
        var extEnumData = schema.Extensions.Where(e => e.Key == "x-enumData").FirstOrDefault();
        if (extEnumData.Value != null)
        {
            var data = extEnumData.Value as OpenApiArray;
            data?.ForEach(item =>
            {
                var prop = new PropertyInfo
                {
                    Name = ((item as OpenApiObject)?["name"] as OpenApiString)?
                        .Value.ToString() ?? "",
                    CommentSummary = ((item as OpenApiObject)?["description"] as OpenApiString)?
                        .Value.ToString() ?? "",

                    Type = "Enum:" + ((item as OpenApiObject)?["value"] as OpenApiInteger)?
                        .Value.ToString() ?? "",
                    IsEnum = true,
                    DefaultValue = ((item as OpenApiObject)?["value"] as OpenApiInteger)?
                        .Value.ToString() ?? "",
                };
                props.Add(prop);
            });
        }
        else if (enums != null)
        {
            for (int i = 0; i < enums.Count; i++)
            {
                var prop = new PropertyInfo
                {
                    Name = (enums[i] as OpenApiInteger)?.Value.ToString() ?? i.ToString(),
                    Type = "Enum:int",
                    IsEnum = true,
                    DefaultValue = (enums[i] as OpenApiInteger)?.Value.ToString() ?? i.ToString(),
                };

                if (extEnum.Value is OpenApiArray values)
                {
                    prop.CommentSummary = (values[i] as OpenApiString)!.Value;
                    prop.Name = (values[i] as OpenApiString)!.Value;
                }
                else
                {
                    prop.CommentSummary = (enums[i] as OpenApiInteger)?.Value.ToString() ?? i.ToString();
                    prop.Name = (enums[i] as OpenApiInteger)?.Value.ToString() ?? i.ToString();
                }
                props.Add(prop);
            }
        }
        return props;
    }

    /// <summary>
    /// 获取所有属性
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static List<PropertyInfo> ParseProperties(OpenApiSchema schema)
    {
        List<PropertyInfo> properties = [];
        // 继承的需要递归 从AllOf中获取属性
        if (schema.AllOf.Count > 1)
        {
            // 自己的属性在1中
            properties.AddRange(ParseProperties(schema.AllOf[1]));
        }

        if (schema.Properties.Count > 0)
        {
            // 泛型处理
            foreach (KeyValuePair<string, OpenApiSchema> prop in schema.Properties)
            {
                string type = GetTypeDescription(prop.Value);
                string name = prop.Key;

                PropertyInfo property = new()
                {
                    IsNullable = prop.Value.Nullable,
                    Name = name,
                    Type = type,
                    IsRequired = !prop.Value.Nullable,
                    MinLength = prop.Value.MinLength,
                    MaxLength = prop.Value.MaxLength,
                    DefaultValue = prop.Value.Default?.ToString() ?? string.Empty,

                };
                if (!string.IsNullOrEmpty(prop.Value.Description))
                {
                    property.CommentSummary = prop.Value.Description;
                }

                // 是否是关联属性
                OpenApiSchema? refType = prop.Value.OneOf?.FirstOrDefault();
                // 列表中的类型
                if (prop.Value.Items?.Reference != null)
                {
                    refType = prop.Value.Items;
                }

                if (prop.Value.Items?.OneOf.Count > 0)
                {
                    refType = prop.Value.Items.OneOf.FirstOrDefault();
                }

                if (refType?.Reference != null)
                {
                    property.NavigationName = refType.Reference.Id;
                    property.IsNavigation = true;
                }

                if (prop.Value.Reference != null)
                {
                    property.NavigationName = prop.Value.Reference.Id;
                    property.IsNavigation = true;
                }
                if (prop.Value.Enum.Any() ||
                    refType != null && refType.Enum.Any())
                {
                    property.IsEnum = true;
                }

                // 可空处理
                properties.Add(property);
            }
        }
        // 重写的属性去重
        List<PropertyInfo?> res = properties.GroupBy(p => p.Name)
            .Select(s => s.FirstOrDefault()).ToList();
        return res!;
    }

    /// <summary>
    /// 获取转换成ts的类型
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static string GetTypeDescription(OpenApiSchema prop)
    {
        string? type = "string";
        // 常规类型
        switch (prop.Type)
        {
            case "boolean":
                type = "boolean";
                break;

            case "integer":
                // 看是否为enum
                type = prop.Enum.Count > 0
                    ? prop.Reference?.Id
                    : "number";
                break;
            case "number":
                type = "number";
                break;
            case "string":
                switch (prop.Format)
                {
                    case "guid":
                        break;
                    case "binary":
                        type = "formData";
                        break;
                    case "date-time":
                        type = "Date";
                        break;
                    default:
                        type = "string";
                        break;
                }
                break;
            case "array":
                type = prop.Items.Reference != null
                    ? prop.Items.Reference.Id + "[]"
                    : GetTypeDescription(prop.Items) + "[]";
                break;
            default:
                type = prop.Reference?.Id;
                break;
        }
        // 引用对象
        if (prop.OneOf.Count > 0)
        {
            // 获取引用对象名称
            type = prop.OneOf.First()?.Reference.Id;
        }

        if (prop.Nullable || prop.Reference != null)
        {
            //type += " | null";
        }

        return type ?? "string";
    }

    /// <summary>
    /// 解析schema类型
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static (string type, string? refType) GetParamType(OpenApiSchema? schema)
    {
        if (schema == null)
        {
            return (string.Empty, string.Empty);
        }

        string type = "any";
        string? refType = schema.Reference?.Id;
        if (schema.Reference != null)
        {
            return (schema.Reference.Id, schema.Reference.Id);
        }
        // 常规类型
        switch (schema.Type)
        {
            case "boolean":
                type = "boolean";
                break;
            case "integer":
                // 看是否为enum
                if (schema.Enum.Count > 0)
                {
                    if (schema.Reference != null)
                    {
                        type = schema.Reference.Id;
                        refType = schema.Reference.Id;
                    }
                }
                else
                {
                    type = "number";
                    refType = "number";
                }
                break;
            case "file":
                type = "FormData";
                break;
            case "string":
                type = "string";
                if (!string.IsNullOrWhiteSpace(schema.Format))
                {
                    type = schema.Format switch
                    {
                        "binary" => "FormData",
                        "date-time" => "string",
                        _ => "string",
                    };
                }

                break;

            case "array":
                if (schema.Items.Reference != null)
                {
                    refType = schema.Items.Reference.Id;
                    type = refType + "[]";
                }
                else if (schema.Items.Type != null)
                {
                    // 基础类型处理
                    refType = schema.Items.Type;
                    refType = refType switch
                    {
                        "integer" => "number",
                        _ => refType
                    };
                    type = refType + "[]";
                }
                else if (schema.Items.OneOf?.FirstOrDefault()?.Reference != null)
                {
                    refType = schema.Items.OneOf?.FirstOrDefault()!.Reference.Id;
                    type = refType + "[]";
                }
                break;
            case "object":
                OpenApiSchema obj = schema.Properties.FirstOrDefault().Value;
                if (obj != null)
                {
                    if (obj.Format == "binary")
                    {
                        type = "FormData";
                    }
                }

                // TODO:object  字典
                if (schema.AdditionalProperties != null)
                {
                    var (inType, inRefType) = GetParamType(schema.AdditionalProperties);
                    refType = inRefType;
                    type = $"Map<string, {inType}>";
                }
                break;
            default:
                break;
        }
        // 引用对象
        if (schema.OneOf.Count > 0)
        {
            // 获取引用对象名称
            type = schema.OneOf.First()?.Reference.Id ?? type;
            refType = schema.OneOf.First()?.Reference.Id;
        }
        return (type, refType);
    }
}
