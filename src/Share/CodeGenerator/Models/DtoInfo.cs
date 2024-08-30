namespace CodeGenerator.Models;

public class DtoInfo
{
    public string? Name { get; set; }
    public string? BaseType { get; set; }
    public List<PropertyInfo>? Properties { get; set; }
    public string? Tag { get; set; }
    public string? NamespaceName { get; set; }
    public string? Comment { get; set; }
    /// <summary>
    /// 原始实体的命名空间完整名称
    /// </summary>
    public required string EntityNamespace { get; set; }

    /// <summary>
    /// dto class content
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="entityName"></param>
    /// <param name="isInput"></param>
    /// <returns></returns>
    public string ToDtoContent(string? projectName = "Share", string entityName = "", bool isInput = false)
    {
        string[] props = Properties?.Select(p => p.ToCsharpLine(isInput)).ToArray()
            ?? [];
        string propStrings = string.Join(string.Empty, props);

        // 对region进行处理
        int regionCount = propStrings.Split("#region").Length - 1;
        int endregionCount = propStrings.Split("#endregion").Length - 1;
        if (endregionCount < regionCount)
        {
            propStrings += Environment.NewLine + "\t#endregion";
        }

        string baseType = string.IsNullOrEmpty(BaseType) ? "" : " : " + BaseType;
        string tpl = $@"using {NamespaceName};
namespace {projectName}.Models.{entityName}Dtos;
{Comment}
/// <see cref=""{EntityNamespace}""/>
public class {Name}{baseType}
{{
{propStrings}    
}}
";
        return tpl;
    }
}
