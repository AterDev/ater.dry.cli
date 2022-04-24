using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Path = System.IO.Path;

namespace CodeGenerator.Generate;

/// <summary>
/// angular request service
/// </summary>
public class NgServiceGenerate : GenerateBase
{
    protected OpenApiPaths PathsPairs { get; }
    public NgServiceGenerate(OpenApiPaths paths)
    {
        PathsPairs = paths;
    }
    public static string GetBaseService()
    {
        var content = GetTplContent("angular.base.service.tpl");
        return content;
    }
    public List<GenFileInfo> GetServices(IList<OpenApiTag> tags)
    {
        var files = new List<GenFileInfo>();

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
                (function.RequestType, function.RequestRefType) = GetParamType(operation.Value.RequestBody.Content.Values.FirstOrDefault()?.Schema);
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
        // 生成文件
        var ngServices = new List<RequestServiceFile>();
        // 先以tag分组
        var funcGroups = functions.GroupBy(f => f.Tag).ToList();
        foreach (var group in funcGroups)
        {
            // 查询该标签包含的所有方法
            var tagFunctions = group.ToList();
            var currentTag = tags.Where(t => t.Name == group.Key).FirstOrDefault();
            if (currentTag == null)
                currentTag = new OpenApiTag { Name = group.Key, Description = group.Key };
            var ngServiceFile = new RequestServiceFile
            {
                Description = currentTag.Description,
                Name = currentTag.Name!,
                Functions = tagFunctions
            };
            var content = ngServiceFile.ToNgService();
            var fileName = currentTag.Name?.ToHyphen() + ".service.ts";

            var file = new GenFileInfo(content)
            {
                Name = fileName,
            };
            files.Add(file);
        }
        return files;
    }

    private static (string? type, string? refType) GetParamType(OpenApiSchema? schema)
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
}


