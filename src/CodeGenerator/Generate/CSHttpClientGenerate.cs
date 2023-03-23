using System.Data;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace CodeGenerator.Generate;
/// <summary>
/// c#请求客户端生成
/// </summary>
public class CSHttpClientGenerate : GenerateBase
{
    protected OpenApiPaths PathsPairs { get; }
    protected List<OpenApiTag> ApiTags { get; }
    public IDictionary<string, OpenApiSchema> Schemas { get; set; }
    public OpenApiDocument OpenApi { get; set; }

    public CSHttpClientGenerate(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        PathsPairs = openApi.Paths;
        Schemas = openApi.Components.Schemas;
        ApiTags = openApi.Tags.ToList();
    }


    public static string GetBaseService()
    {
        return default!;
    }
    public static string GetClient()
    {
        return default!;
    }
    public static string GetGlobalUsing()
    {
        return default!;
    }



    public List<GenFileInfo> GetServices(IList<OpenApiTag> tags)
    {
        List<GenFileInfo> files = new();
        List<RequestServiceFunction> functions = GetAllRequestFunctions();

        // 先以tag分组
        List<IGrouping<string?, RequestServiceFunction>> funcGroups = functions.GroupBy(f => f.Tag).ToList();
        foreach (IGrouping<string?, RequestServiceFunction>? group in funcGroups)
        {
            // 查询该标签包含的所有方法
            List<RequestServiceFunction> tagFunctions = group.ToList();
            OpenApiTag? currentTag = tags.Where(t => t.Name == group.Key).FirstOrDefault();
            currentTag ??= new OpenApiTag { Name = group.Key, Description = group.Key };
            RequestServiceFile serviceFile = new()
            {
                Description = currentTag.Description,
                Name = currentTag.Name!,
                Functions = tagFunctions
            };

            string content = ToRequestService(serviceFile);

            string fileName = currentTag.Name?.ToHyphen() + ".service.ts";
            GenFileInfo file = new(fileName, content);

            files.Add(file);
        }
        return files;
    }

    public string ToRequestService(RequestServiceFile serviceFile)
    {
        var functions = serviceFile.Functions;
        string functionstr = "";
        // import引用的models
        string importModels = "";
        List<string> refTypes = new();
        if (functions != null)
        {
            functionstr = string.Join("\n", functions.Select(f => f.ToNgRequestFunction()).ToArray());
            string[] baseTypes = new string[] { "string", "string[]", "number", "number[]", "boolean", "integer" };
            // 获取请求和响应的类型，以便导入
            List<string?> requestRefs = functions
                .Where(f => !string.IsNullOrEmpty(f.RequestRefType)
                    && !baseTypes.Contains(f.RequestRefType))
                .Select(f => f.RequestRefType).ToList();
            List<string?> responseRefs = functions
                .Where(f => !string.IsNullOrEmpty(f.ResponseRefType)
                    && !baseTypes.Contains(f.ResponseRefType))
                .Select(f => f.ResponseRefType).ToList();

            // 参数中的类型
            List<string?> paramsRefs = functions.SelectMany(f => f.Params!)
                .Where(p => !baseTypes.Contains(p.Type))
                .Select(p => p.Type)
                .ToList();
            if (requestRefs != null)
            {
                refTypes.AddRange(requestRefs!);
            }

            if (responseRefs != null)
            {
                refTypes.AddRange(responseRefs!);
            }

            if (paramsRefs != null)
            {
                refTypes.AddRange(paramsRefs!);
            }

            refTypes = refTypes.GroupBy(t => t)
                .Select(g => g.FirstOrDefault()!)
                .ToList();

            refTypes.ForEach(t =>
            {
                if (Config.EnumModels.Contains(t))
                {
                    importModels += $"import {{ {t} }} from '../models/enum/{t.ToHyphen()}.model';{Environment.NewLine}";
                }
                else
                {
                    string? dirName = "";
                    importModels += $"import {{ {t} }} from '../models/{dirName?.ToHyphen()}/{t.ToHyphen()}.model';{Environment.NewLine}";
                }

            });
        }
        string result = $@"import {{ Injectable }} from '@angular/core';
import {{ BaseService }} from './base.service';
import {{ Observable }} from 'rxjs';
{importModels}
/**
 * {serviceFile.Description}
 */
@Injectable({{ providedIn: 'root' }})
export class {serviceFile.Name}Service extends BaseService {{
{functionstr}
}}
";
        return result;
    }

    /// <summary>
    /// 获取方法信息
    /// </summary>
    /// <returns></returns>
    public List<RequestServiceFunction> GetAllRequestFunctions()
    {
        List<RequestServiceFunction> functions = new();
        // 处理所有方法
        foreach (KeyValuePair<string, OpenApiPathItem> path in PathsPairs)
        {
            foreach (KeyValuePair<OperationType, OpenApiOperation> operation in path.Value.Operations)
            {
                RequestServiceFunction function = new()
                {
                    Description = operation.Value.Summary,
                    Method = operation.Key.ToString(),
                    Name = operation.Value.OperationId,
                    Path = path.Key,
                    Tag = operation.Value.Tags.FirstOrDefault()?.Name,
                };
                (function.RequestType, function.RequestRefType) = GetCsharpParamType(operation.Value.RequestBody?.Content.Values.FirstOrDefault()?.Schema);
                (function.ResponseType, function.ResponseRefType) = GetCsharpParamType(operation.Value.Responses.FirstOrDefault().Value
                    ?.Content.FirstOrDefault().Value
                    ?.Schema);
                function.Params = operation.Value.Parameters?.Select(p =>
                {
                    string? location = p.In?.GetDisplayName();
                    bool? inpath = location?.ToLower()?.Equals("path");
                    (string type, string _) = GetCsharpParamType(p.Schema);
                    return new FunctionParams
                    {
                        Description = p.Description,
                        Name = p.Name,
                        InPath = inpath ?? false,
                        IsRequired = p.Required,
                        Type = type
                    };
                }).ToList();

                functions.Add(function);
            }
        }
        return functions;
    }

    /// <summary>
    /// 获取C#参数类型
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static (string type, string? refType) GetCsharpParamType(OpenApiSchema? schema)
    {
        if (schema == null)
        {
            return (string.Empty, string.Empty);
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
                    var (inType, inRefType) = GetCsharpParamType(schema.AdditionalProperties);
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
