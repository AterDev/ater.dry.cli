using System.Security.Principal;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using PropertyInfo = Core.Models.PropertyInfo;

namespace Core.Infrastructure.Helper;
/// <summary>
/// openapi 解析帮助类
/// </summary>
public class OpenApiHelper
{
    public OpenApiDocument OpenApi { get; set; }
    /// <summary>
    /// 接口信息
    /// </summary>
    public List<RestApiInfo> RestApiInfos { get; set; }
    /// <summary>
    /// 所有请求及返回类型信息
    /// </summary>
    public List<EntityInfo> EntityInfos { get; set; }
    /// <summary>
    /// tag信息
    /// </summary>
    public List<OpenApiTag> OpenApiTags { get; set; }

    public OpenApiHelper(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        OpenApiTags = openApi.Tags.ToList();

    }

    public List<RestApiInfo> GetRestApiInfos()
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
                    // TODO:关联的类型
                }
                if (responseBody != null)
                {
                    var (ResponseType, ResponseRefType) = GetParamType(responseBody
                       .FirstOrDefault().Value?.Content
                       .FirstOrDefault().Value?.Schema);

                    // TODO:关联的类型

                }
                if (requestParameters != null)
                {
                    var parammeters = requestParameters?.Select(p =>
                    {
                        string? location = p.In?.GetDisplayName();
                        bool? inpath = location?.ToLower()?.Equals("path");
                        (string type, string _) = GetParamType(p.Schema);
                        return new PropertyInfo
                        {
                            ProjectId = Const.PROJECT_ID,
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

        return apiInfos;
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
            string description = schema.Value.AllOf.LastOrDefault()?.Description
                ?? schema.Value.Description;

            description = description?.Replace("\n", " ") ?? "";
            if (!string.IsNullOrEmpty(description))
            {
                description = $"({description})".Replace(" = ", "=");
            }

            //List<TsProperty> props = TSModelGenerate.GetTsProperties(schema.Value);
        }
        return models;
    }


    /// <summary>
    /// 获取所有属性
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static List<PropertyInfo> GetTsProperties(OpenApiSchema schema)
    {
        List<PropertyInfo> tsProperties = new();
        // 继承的需要递归 从AllOf中获取属性
        if (schema.AllOf.Count > 1)
        {
            // 自己的属性在1中
            tsProperties.AddRange(GetTsProperties(schema.AllOf[1]));
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
                    ProjectId = Const.PROJECT_ID,
                    IsNullable = prop.Value.Nullable,
                    Name = name,
                    Type = type,
                    IsRequired = !prop.Value.Nullable
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
                    (refType != null && refType.Enum.Any()))
                {
                    property.IsEnum = true;
                }

                // 可空处理
                tsProperties.Add(property);
            }
        }
        // 重写的属性去重
        List<PropertyInfo?> res = tsProperties.GroupBy(p => p.Name)
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
            type += " | null";
        }

        return type ?? "string";
    }

    public static (string type, string? refType) GetParamType(OpenApiSchema? schema)
    {
        if (schema == null)
        {
            return ("any", "any");
        }

        string? type = "any";
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
                }
                break;
            case "file":
                type = "FormData";
                break;
            case "string":
                type = schema.Format switch
                {
                    "binary" => "FormData",
                    "date-time" => "string",
                    _ => "string",
                };
                break;

            case "array":
                if (schema.Items.Reference != null)
                {
                    refType = schema.Items.Reference.Id;
                    type = refType + "[]";
                }
                else if (schema.Items.Type != null)
                {
                    // 基础类型?
                    refType = schema.Items.Type;
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
