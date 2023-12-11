namespace Core.Models;
/// <summary>
/// 服务文件
/// </summary>
public class RequestServiceFile
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public List<RequestServiceFunction>? Functions { get; set; }

    /// <summary>
    /// 生成angluar http client service
    /// </summary>
    /// <returns></returns>
    public string ToNgService()
    {
        string functions = "";
        // import引用的models
        string importModels = "";
        List<string> refTypes = [];
        if (Functions != null)
        {
            functions = string.Join("\n", Functions.Select(f => f.ToNgRequestFunction()).ToArray());
            string[] baseTypes = ["string", "string[]", "number", "number[]", "boolean"];
            // 获取请求和响应的类型，以便导入
            List<string?> requestRefs = Functions
                .Where(f => !string.IsNullOrEmpty(f.RequestRefType)
                    && !baseTypes.Contains(f.RequestRefType))
                .Select(f => f.RequestRefType).ToList();
            List<string?> responseRefs = Functions
                .Where(f => !string.IsNullOrEmpty(f.ResponseRefType)
                    && !baseTypes.Contains(f.ResponseRefType))
                .Select(f => f.ResponseRefType).ToList();

            // 参数中的类型
            List<string?> paramsRefs = Functions.SelectMany(f => f.Params!)
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
                    importModels += $"import {{ {t} }} from '../models/{Name.ToHyphen()}/{t.ToHyphen()}.model';{Environment.NewLine}";
                }

            });
        }
        string result = $@"import {{ Injectable }} from '@angular/core';
import {{ BaseService }} from './base.service';
import {{ Observable }} from 'rxjs';
{importModels}
/**
 * {Description}
 */
@Injectable({{ providedIn: 'root' }})
export class {Name}Service extends BaseService {{
{functions}
}}
";
        return result;
    }

}

/// <summary>
/// 请求服务的函数
/// </summary>
public class RequestServiceFunction
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Method { get; set; } = default!;
    public string? ResponseType { get; set; }
    /// <summary>
    /// 返回中的引用类型
    /// </summary>
    public string? ResponseRefType { get; set; }
    public string RequestType { get; set; } = string.Empty;
    /// <summary>
    /// 请求中的引用类型
    /// </summary>
    public string? RequestRefType { get; set; }
    /// <summary>
    /// 参数及类型
    /// </summary>
    public List<FunctionParams>? Params { get; set; }
    /// <summary>
    /// 相对请求路径
    /// </summary>
    public string Path { get; set; } = default!;
    /// <summary>
    /// 标签
    /// </summary>
    public string? Tag { get; set; }

    public string ToNgRequestFunction()
    {
        ResponseType = string.IsNullOrWhiteSpace(ResponseType) ? "any" : ResponseType;
        // 函数名处理，去除tag前缀，然后格式化
        Name = Name.Replace(Tag + "_", "");
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
   * {Description ?? Name}
{paramsComments}   */";

        // 构造请求url
        List<string?>? paths = Params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
            {
                // 路由参数处理
                string origin = $"{{{p}}}";
                Path = Path.Replace(origin, "$" + origin);
            });

        // 需要拼接的参数,特殊处理文件上传
        List<string?>? reqParams = Params?.Where(p => !p.InPath && p.Type != "FormData")
            .Select(p => p.Name)?.ToList();
        if (reqParams != null)
        {
            string queryParams = "";
            queryParams = string.Join("&", reqParams.Select(p => { return $"{p}=${{{p} ?? ''}}"; }).ToArray());
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

        string function = @$"{comments}
  {Name}({paramsString}): Observable<{ResponseType}> {{
    const _url = `{Path}`;
    return this.{method}{generics}('{Method.ToLower()}', _url{dataString});
  }}
";
        return function;
    }
}

/// <summary>
/// 函数参数
/// </summary>
public class FunctionParams
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = true;
    /// <summary>
    /// 是否路由参数
    /// </summary>
    public bool InPath { get; set; }
}
