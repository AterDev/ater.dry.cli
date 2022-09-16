using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace CodeGenerator.Generate;
/// <summary>
/// 请求生成
/// </summary>
public class RequestGenearte : GenerateBase
{
    protected OpenApiPaths PathsPairs { get; }
    protected List<OpenApiTag> ApiTags { get; }
    public IDictionary<string, OpenApiSchema> Schemas { get; set; }
    public OpenApiDocument OpenApi { get; set; }

    /// <summary>
    /// 模型类字典
    /// </summary>
    public Dictionary<string, string?> ModelDictionary { get; set; } = new();
    public RequestLibType LibType { get; set; } = RequestLibType.NgHttp;

    public RequestGenearte(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        PathsPairs = openApi.Paths;
        // 构建模型名及对应Tag目录的字典
        openApi.Components.Schemas.Keys.ToList()
            .ForEach(k =>
            {
                ModelDictionary[k] = null;
            });

        Schemas = openApi.Components.Schemas;
        ApiTags = openApi.Tags.ToList();
    }

    public static string GetBaseService(RequestLibType libType)
    {
        var content = libType switch
        {
            RequestLibType.NgHttp => GetTplContent("angular.base.service.tpl"),
            RequestLibType.Axios => GetTplContent("RequestService.axios.service.tpl"),
            _ => ""
        };

        return content;
    }

    /// <summary>
    /// 获取所有请求接口解析的函数结构
    /// </summary>
    /// <returns></returns>
    public List<RequestServiceFunction> GetAllRequestFunctions()
    {
        var functions = new List<RequestServiceFunction>();
        // 处理所有方法
        foreach (var path in PathsPairs)
        {
            foreach (var operation in path.Value.Operations)
            {
                var function = new RequestServiceFunction
                {
                    Description = operation.Value.Summary,
                    Method = operation.Key.ToString(),
                    Name = operation.Value.OperationId,
                    Path = path.Key,
                    Tag = operation.Value.Tags.FirstOrDefault()?.Name,
                };
                (function.RequestType, function.RequestRefType) = GetParamType(operation.Value.RequestBody?.Content.Values.FirstOrDefault().Schema);
                (function.ResponseType, function.ResponseRefType) = GetParamType(operation.Value.Responses.FirstOrDefault().Value
                    ?.Content.FirstOrDefault().Value
                    ?.Schema);
                function.Params = operation.Value.Parameters?.Select(p =>
                {
                    var location = p.In?.GetDisplayName();
                    var inpath = location?.ToLower()?.Equals("path");
                    var (type, _) = GetParamType(p.Schema);
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
    /// 根据tag生成多个请求服务文件
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public List<GenFileInfo> GetServices(IList<OpenApiTag> tags)
    {
        var files = new List<GenFileInfo>();
        var functions = GetAllRequestFunctions();

        // 先以tag分组
        var funcGroups = functions.GroupBy(f => f.Tag).ToList();
        foreach (var group in funcGroups)
        {
            // 查询该标签包含的所有方法
            var tagFunctions = group.ToList();
            var currentTag = tags.Where(t => t.Name == group.Key).FirstOrDefault();
            if (currentTag == null)
                currentTag = new OpenApiTag { Name = group.Key, Description = group.Key };
            var serviceFile = new RequestServiceFile
            {
                Description = currentTag.Description,
                Name = currentTag.Name!,
                Functions = tagFunctions
            };

            var content = LibType switch
            {
                RequestLibType.NgHttp => serviceFile.ToNgService(),
                RequestLibType.Axios => ToAxiosRequestService(serviceFile),
                _ => ""
            };

            var fileName = currentTag.Name?.ToHyphen() + ".service.ts";
            var file = new GenFileInfo(content)
            {
                Name = fileName,
            };
            files.Add(file);
        }
        return files;
    }

    public List<GenFileInfo> GetTSInterfaces()
    {
        var tsGen = new TSModelGenerate(OpenApi);
        var  files = new List<GenFileInfo>();
        foreach (var item in Schemas)
        {
            files.Add(tsGen.GenerateInterfaceFile(item.Key, item.Value));
        }

        return files;

    }

    public static (string? type, string? refType) GetParamType(OpenApiSchema? schema)
    {
        if (schema == null)
            return (string.Empty, string.Empty);

        var type = "any";
        var refType = schema.Reference?.Id;
        if (schema.Reference != null)
            return (schema.Reference.Id, schema.Reference.Id);
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
                var obj = schema.Properties.FirstOrDefault().Value;
                if (obj != null)
                {
                    if (obj.Format == "binary")
                        type = "FormData";
                }
                break;
            default:
                break;
        }
        // 引用对象
        if (schema.OneOf.Count > 0)
        {
            // 获取引用对象名称
            type = schema.OneOf.First()?.Reference.Id;
            refType = schema.OneOf.First()?.Reference.Id;
        }
        return (type, refType);
    }

    public string ToAxiosRequestService(RequestServiceFile serviceFile)
    {
        var tplContent = GetTplContent("RequestService.service.ts");
        var functionString = "";
        var functions = serviceFile.Functions;
        // import引用的models
        var importModels = "";
        if (functions != null)
        {
            functionString = string.Join("\n",
                functions.Select(f => ToAxiosFunction(f)).ToArray());
            var refTypes = GetRefTyeps(functions);
            refTypes.ForEach(t =>
            {
                var dirName = ModelDictionary.GetValueOrDefault(t);
                importModels += $"import {{ {t} }} from '../models/{dirName?.ToHyphen()}/{t.ToHyphen()}.model';{Environment.NewLine}";
            });
        }
        tplContent = tplContent.Replace("[@Import]", importModels)
            .Replace("[@ServiceName]", serviceFile.Name)
            .Replace("[@Functions]", functionString);
        return tplContent;

    }

    /// <summary>
    /// axios函数格式
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    protected string ToAxiosFunction(RequestServiceFunction function)
    {
        var Name = function.Name;
        var Params = function.Params;
        var RequestType = function.RequestType;
        var ResponseType = function.ResponseType;
        var Path = function.Path;

        // 函数名处理，去除tag前缀，然后格式化
        Name = Name.Replace(function.Tag + "_", "");
        Name = Name.ToCamelCase();
        // 处理参数
        var paramsString = "";
        var paramsComments = "";
        var dataString = "";
        if (Params?.Count > 0)
        {
            paramsString = string.Join(", ",
                Params.OrderBy(p => p.IsRequired)
                    .Select(p => p.Name + ": " + p.Type)
                .ToArray());
            Params.ForEach(p =>
            {
                paramsComments += $"   * @param {p.Name} {p.Description ?? p.Type}\n";
            });
        }
        if (!string.IsNullOrEmpty(RequestType))
        {
            if (Params?.Count > 0)
            {
                paramsString += $", data: {RequestType}";
            }
            else
            {
                paramsString = $"data: {RequestType}";
            }

            dataString = ", data";
            paramsComments += $"   * @param data {RequestType}\n";
        }
        // 添加extOptions
        if (!string.IsNullOrWhiteSpace(paramsComments))
        {
            paramsString += ", ";
        }
        paramsString += "extOptions?: ExtOptions";
        // 注释生成
        var comments = $@"  /**
   * {function.Description ?? Name}
{paramsComments}   */";

        // 构造请求url
        var paths = Params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        if (paths != null)
        {
            paths.ForEach(p =>
            {
                var origin = $"{{{p}}}";
                Path = Path.Replace(origin, "$" + origin);
            });
        }
        // 需要拼接的参数,特殊处理文件上传
        var reqParams = Params?.Where(p => !p.InPath && p.Type != "FormData")
            .Select(p => p.Name)?.ToList();
        if (reqParams != null)
        {
            var queryParams = "";
            queryParams = string.Join("&", reqParams.Select(p => { return $"{p}=${{{p}}}"; }).ToArray());
            if (!string.IsNullOrEmpty(queryParams))
                Path += "?" + queryParams;
        }
        // 上传文件时的名称
        var file = Params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
            dataString = $", {file.Name}";

        // 默认添加ext
        if (string.IsNullOrEmpty(dataString))
        {
            dataString = ", null, extOptions";
        }
        else
        {
            dataString += ", extOptions";
        }
        var functionString = @$"{comments}
  {Name}({paramsString}): Promise<{ResponseType}> {{
    const url = `{Path}`;
    return this.request<{ResponseType}>('{function.Method.ToLower()}', url{dataString});
  }}
";
        return functionString;
    }

    /// <summary>
    /// 获取要导入的依赖
    /// </summary>
    /// <param name="functions"></param>
    /// <returns></returns>
    protected List<string> GetRefTyeps(List<RequestServiceFunction> functions)
    {
        var refTypes = new List<string>();

        var baseTypes = new string[] { "string", "string[]", "number", "number[]", "boolean" };
        // 获取请求和响应的类型，以便导入
        var requestRefs = functions
                .Where(f => !string.IsNullOrEmpty(f.RequestRefType)
                    && !baseTypes.Contains(f.RequestRefType))
                .Select(f => f.RequestRefType).ToList();
        var responseRefs = functions
                .Where(f => !string.IsNullOrEmpty(f.ResponseRefType)
                    && !baseTypes.Contains(f.ResponseRefType))
                .Select(f => f.ResponseRefType).ToList();

        // 参数中的类型
        var paramsRefs = functions.SelectMany(f => f.Params!)
                .Where(p => !baseTypes.Contains(p.Type))
                .Select(p => p.Type)
                .ToList();
        if (requestRefs != null) refTypes.AddRange(requestRefs!);
        if (responseRefs != null) refTypes.AddRange(responseRefs!);
        if (paramsRefs != null) refTypes.AddRange(paramsRefs!);

        refTypes = refTypes.GroupBy(t => t)
            .Select(g => g.FirstOrDefault()!)
            .ToList();
        return refTypes;
    }
}
public enum RequestLibType
{
    NgHttp,
    Axios
}