using Datastore;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace CodeGenerator.Generate;
/// <summary>
/// 请求生成
/// </summary>
public class RequestGenerate(OpenApiDocument openApi) : GenerateBase
{
    protected OpenApiPaths PathsPairs { get; } = openApi.Paths;
    protected List<OpenApiTag> ApiTags { get; } = [.. openApi.Tags];
    public IDictionary<string, OpenApiSchema> Schemas { get; set; } = openApi.Components.Schemas;
    public OpenApiDocument OpenApi { get; set; } = openApi;

    public RequestLibType LibType { get; set; } = RequestLibType.NgHttp;
    public string? Server { get; set; } = openApi.Servers.FirstOrDefault()?.Url;

    public List<GenFileInfo>? TsModelFiles { get; set; }

    public static string GetBaseService(RequestLibType libType)
    {
        try
        {
            var db = new DbContext();
            string? content = null;
            var data = db.TemplateFile?.FindAll().ToList();
            switch (libType)
            {
                case RequestLibType.NgHttp:
                    content = data?
                        .Where(t => t.Name == "angular.base.service.tpl")
                        .Where(t => t.ProjectId == Const.PROJECT_ID)
                        .Select(t => t.Content)
                        .FirstOrDefault();
                    return content ?? GetTplContent("angular.base.service.tpl");

                case RequestLibType.Axios:
                    content = data?
                        .Where(t => t.Name == "RequestService.axios.service.tpl")
                        .Where(t => t.ProjectId == Const.PROJECT_ID)
                        .Select(t => t.Content)
                        .FirstOrDefault();
                    return content ?? GetTplContent("RequestService.axios.service.tpl");
                default:
                    break;
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine("request base service:" + ex.Message + ex.StackTrace + ex.InnerException);
            return default!;
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取所有请求接口解析的函数结构
    /// </summary>
    /// <returns></returns>
    public List<RequestServiceFunction> GetAllRequestFunctions()
    {
        List<RequestServiceFunction> functions = [];
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
                if (string.IsNullOrWhiteSpace(function.Name))
                {
                    function.Name = operation.Key + "_" + path.Key.Split("/").LastOrDefault();
                }
                (function.RequestType, function.RequestRefType) = GetTypescriptParamType(operation.Value.RequestBody?.Content.Values.FirstOrDefault()?.Schema);
                (function.ResponseType, function.ResponseRefType) = GetTypescriptParamType(operation.Value.Responses.FirstOrDefault().Value
                    ?.Content.FirstOrDefault().Value
                    ?.Schema);
                function.Params = operation.Value.Parameters?.Select(p =>
                {
                    string? location = p.In?.GetDisplayName();
                    bool? inpath = location?.ToLower()?.Equals("path");
                    (string type, string? _) = GetTypescriptParamType(p.Schema);
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
        if (TsModelFiles == null)
        {
            GetTSInterfaces();
        }
        List<GenFileInfo> files = [];
        List<RequestServiceFunction> functions = GetAllRequestFunctions();

        // 先以tag分组
        List<IGrouping<string?, RequestServiceFunction>> funcGroups = functions.GroupBy(f => f.Tag).ToList();
        foreach (IGrouping<string?, RequestServiceFunction>? group in funcGroups)
        {
            // 查询该标签包含的所有方法
            List<RequestServiceFunction> tagFunctions = [.. group];
            OpenApiTag? currentTag = tags.Where(t => t.Name == group.Key).FirstOrDefault();
            currentTag ??= new OpenApiTag { Name = group.Key, Description = group.Key };
            RequestServiceFile serviceFile = new()
            {
                Description = currentTag.Description,
                Name = currentTag.Name!,
                Functions = tagFunctions
            };

            string content = LibType switch
            {
                RequestLibType.NgHttp => ToNgRequestBaseService(serviceFile),
                RequestLibType.Axios => ToAxiosRequestService(serviceFile),
                _ => ""
            };

            switch (LibType)
            {
                // 同时生成基类和继承类，继承类可自定义
                case RequestLibType.NgHttp:
                {
                    string baseFileName = currentTag.Name?.ToHyphen() + "-base.service.ts";
                    GenFileInfo file = new(baseFileName, content);
                    files.Add(file);

                    string fileName = currentTag.Name?.ToHyphen() + ".service.ts";
                    content = ToNgRequestService(serviceFile);
                    file = new(fileName, content)
                    {
                        CanModify = true
                    };
                    files.Add(file);
                    break;
                }
                case RequestLibType.Axios:
                {
                    string fileName = currentTag.Name?.ToHyphen() + ".service.ts";
                    GenFileInfo file = new(fileName, content);
                    files.Add(file);
                    break;
                }
                default:
                    break;
            }


        }
        return files;
    }

    /// <summary>
    /// ts interface files
    /// </summary>
    /// <returns></returns>
    public List<GenFileInfo> GetTSInterfaces()
    {
        TSModelGenerate tsGen = new(OpenApi);
        List<GenFileInfo> files = [];
        foreach (KeyValuePair<string, OpenApiSchema> item in Schemas)
        {
            files.Add(tsGen.GenerateInterfaceFile(item.Key, item.Value));
        }
        TsModelFiles = files;
        return files;
    }

    public static string GetEnumPipeContent(IDictionary<string, OpenApiSchema> schemas)
    {
        string tplContent = GetTplContent("angular.enum.pipe.ts");
        string codeBlocks = "";
        foreach (KeyValuePair<string, OpenApiSchema> item in schemas)
        {
            if (item.Value.Enum.Count > 0)
            {
                codeBlocks += ToEnumSwitchString(item.Key, item.Value);
            }
        }
        tplContent = tplContent.Replace("${EnumBlocks}", codeBlocks);
        return tplContent;
    }

    public static string ToEnumSwitchString(string enumType, OpenApiSchema schema)
    {
        KeyValuePair<string, IOpenApiExtension> enumData = schema.Extensions
                .Where(e => e.Key == "x-enumData")
                .FirstOrDefault();
        // 过滤没有注释的内容
        if (enumData.Value == null) return string.Empty;
        if (enumData.Value is OpenApiArray data)
        {
            if (!data.Any()) { return string.Empty; }
        }

        var sb = new StringBuilder();

        TabConst.TabSize = 2;
        sb.AppendLine($"case '{enumType}':".Indent(3));
        sb.AppendLine("{".Indent(4));
        sb.AppendLine($"  switch (value)".Indent(5));
        sb.AppendLine("{".Indent(4));

        if (enumData.Value is OpenApiArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                var item = (OpenApiObject)array[i];

                string caseString = string.Format("case {0}: result = '{1}'; break;".Indent(6), ((OpenApiInteger)item["value"]).Value, ((OpenApiString)item["description"]).Value);

                sb.AppendLine(caseString);
            }
            sb.AppendLine("default: '默认'; break;".Indent(6));
        }

        sb.AppendLine("}".Indent(5));
        sb.AppendLine("}".Indent(4));
        sb.AppendLine("break;".Indent(4));
        return sb.ToString();
    }

    /// <summary>
    /// 解析参数及类型
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static (string type, string? refType) GetTypescriptParamType(OpenApiSchema? schema)
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
            case "number":
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
                    var (inType, inRefType) = GetTypescriptParamType(schema.AdditionalProperties);
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

    public string ToAxiosRequestService(RequestServiceFile serviceFile)
    {
        string tplContent = GetTplContent("RequestService.service.ts");
        string functionString = "";
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        // import引用的models
        string importModels = "";
        if (functions != null)
        {
            functionString = string.Join("\n",
                functions.Select(ToAxiosFunction).ToArray());
            List<string> refTypes = GetRefTyeps(functions);
            refTypes.ForEach(t =>
            {
                string? dirName = TsModelFiles?.Where(f => f.ModelName == t).Select(f => f.Path).FirstOrDefault();
                importModels += $"import {{ {t} }} from '../models/{dirName?.ToHyphen()}/{t.ToHyphen()}.model';{Environment.NewLine}";
            });
        }
        tplContent = tplContent.Replace("[@Import]", importModels)
            .Replace("[@ServiceName]", serviceFile.Name)
            .Replace("[@Functions]", functionString);
        return tplContent;
    }

    /// <summary>
    /// 生成angular请求服务基类
    /// </summary>
    /// <param name="serviceFile"></param>
    /// <returns></returns>
    public string ToNgRequestBaseService(RequestServiceFile serviceFile)
    {
        var functions = serviceFile.Functions;
        string functionstr = "";
        // import引用的models
        string importModels = "";
        List<string> refTypes = [];
        if (functions != null)
        {
            functionstr = string.Join("\n", functions.Select(ToNgRequestFunction).ToArray());
            string[] baseTypes = ["string", "string[]", "number", "number[]", "boolean", "integer"];
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
                    string? dirName = TsModelFiles?.Where(f => f.ModelName == t).Select(f => f.Path).FirstOrDefault();
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
export class {serviceFile.Name}BaseService extends BaseService {{
{functionstr}
}}
";
        return result;
    }

    /// <summary>
    /// 生成ng 请求服务继承类,可自定义
    /// </summary>
    /// <param name="serviceFile"></param>
    /// <returns></returns>
    public static string ToNgRequestService(RequestServiceFile serviceFile)
    {
        string result = $$"""
import { Injectable } from '@angular/core';
import { {{serviceFile.Name}}BaseService } from './{{serviceFile.Name.ToHyphen()}}-base.service';

/**
 * {{serviceFile.Description}}
 */
@Injectable({providedIn: 'root' })
export class {{serviceFile.Name}}Service extends {{serviceFile.Name}}BaseService {
  id: string | null = null;
  name: string | null = null;
}
""";
        return result;
    }

    /// <summary>
    /// axios函数格式
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    protected string ToAxiosFunction(RequestServiceFunction function)
    {
        string Name = function.Name;
        List<FunctionParams>? Params = function.Params;
        string RequestType = function.RequestType;
        string ResponseType = string.IsNullOrWhiteSpace(function.ResponseType) ? "any" : function.ResponseType!;

        string Path = function.Path;
        if (Server != null)
        {
            Path = Server + Path;
        }

        // 函数名处理，去除tag前缀，然后格式化
        Name = Name.Replace(function.Tag + "_", "");
        Name = Name.ToCamelCase();
        // 处理参数
        string paramsString = "";
        string paramsComments = "";
        string dataString = "";
        if (Params?.Count > 0)
        {
            paramsString = string.Join(", ",
                Params.OrderByDescending(p => p.IsRequired)
                    .Select(p => p.IsRequired
                        ? p.Name + ": " + p.Type
                        : p.Name + ": " + p.Type + " | null")
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
        string comments = $@"  /**
   * {function.Description ?? Name}
{paramsComments}   */";

        // 构造请求url
        List<string?>? paths = Params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
            {
                string origin = $"{{{p}}}";
                Path = Path.Replace(origin, "$" + origin);
            });
        // 需要拼接的参数,特殊处理文件上传
        List<string?>? reqParams = Params?.Where(p => !p.InPath && p.Type != "FormData")
            .Select(p => p.Name)?.ToList();
        if (reqParams != null)
        {
            string queryParams = "";
            queryParams = string.Join("&", reqParams.Select(p =>
            {
                return $"{p}=${{{p} ?? ''}}";
            }).ToArray());
            if (!string.IsNullOrEmpty(queryParams))
            {
                Path += "?" + queryParams;
            }
        }
        // 上传文件时的名称
        FunctionParams? file = Params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
        {
            dataString = $", {file.Name}";
        }

        // 默认添加ext
        if (string.IsNullOrEmpty(dataString))
        {
            dataString = ", null, extOptions";
        }
        else
        {
            dataString += ", extOptions";
        }
        string functionString = @$"{comments}
  {Name}({paramsString}): Promise<{ResponseType}> {{
    const _url = `{Path}`;
    return this.request<{ResponseType}>('{function.Method.ToLower()}', _url{dataString});
  }}
";
        return functionString;
    }

    public string ToNgRequestFunction(RequestServiceFunction function)
    {
        string Name = function.Name;
        List<FunctionParams>? Params = function.Params;
        string RequestType = function.RequestType;
        string ResponseType = string.IsNullOrWhiteSpace(function.ResponseType) ? "any" : function.ResponseType!;

        string Path = function.Path;
        if (Server != null)
        {
            Path = Server + Path;
        }

        // 函数名处理，去除tag前缀，然后格式化
        Name = Name.Replace(function.Tag + "_", "");
        Name = Name.ToCamelCase();

        // 处理参数
        string paramsString = "";
        string paramsComments = "";
        string dataString = "";
        if (Params?.Count > 0)
        {
            paramsString = string.Join(", ",
                Params.OrderByDescending(p => p.IsRequired)
                    .Select(p => p.IsRequired
                        ? p.Name + ": " + p.Type
                        : p.Name + ": " + p.Type + " | null")
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
        // 注释生成
        string comments = $@"  /**
   * {function.Description ?? Name}
{paramsComments}   */";

        // 构造请求url
        List<string?>? paths = Params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
        {
            string origin = $"{{{p}}}";
            Path = Path.Replace(origin, "$" + origin);
        });
        // 需要拼接的参数,特殊处理文件上传
        List<string?>? reqParams = Params?.Where(p => !p.InPath && p.Type != "FormData")
            .Select(p => p.Name)?.ToList();
        if (reqParams != null)
        {
            string queryParams = "";
            queryParams = string.Join("&", reqParams.Select(p =>
            {
                return $"{p}=${{{p} ?? ''}}";
            }).ToArray());
            if (!string.IsNullOrEmpty(queryParams))
            {
                Path += "?" + queryParams;
            }
        }
        FunctionParams? file = Params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
        {
            dataString = $", {file.Name}";
        }

        var method = "request";
        var generics = $"<{ResponseType}>";
        if (ResponseType.Equals("FormData"))
        {
            ResponseType = "Blob";
            method = "downloadFile";
            generics = "";
        }

        string functionString = @$"{comments}
  {Name}({paramsString}): Observable<{ResponseType}> {{
    const _url = `{Path}`;
    return this.{method}{generics}('{function.Method.ToLower()}', _url{dataString});
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
        List<string> refTypes = [];

        string[] baseTypes = ["string", "string[]", "number", "number[]", "boolean", "integer"];
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
        return refTypes;
    }
}
public enum RequestLibType
{
    NgHttp,
    Axios
}