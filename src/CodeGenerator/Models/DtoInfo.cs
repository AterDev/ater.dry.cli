namespace CodeGenerator.Models;

public class DtoInfo
{
    public string? Name { get; set; }
    public string? BaseType { get; set; }
    public List<PropertyInfo>? Properties { get; set; }
    public string? Tag { get; set; }
    public string? NamespaceName { get; set; }
    public string? Comment { get; set; }

    public string ToString(string? projectName = "Share", string entityName = "")
    {
        var props = Properties?.Select(p => p.ToCsharpLine()).ToArray()
            ?? Array.Empty<string>();
        var propStrings = string.Join(string.Empty, props);

        // 对region进行处理
        var regionCount = propStrings.Split("#region").Length - 1;
        var endregionCount = propStrings.Split("#endregion").Length - 1;
        if (endregionCount < regionCount)
        {
            propStrings += Environment.NewLine + "\t#endregion";
        }

        var baseType    = string.IsNullOrEmpty(BaseType) ? "" : " : " + BaseType;
        var tpl         = $@"namespace {projectName}.Models.{entityName}Dtos;
{Comment}
public class {Name}{baseType}
{{
{propStrings}    
}}
";
        return tpl;
    }
    public void Save(string dir, bool cover)
    {
        var path = Path.Combine(dir, Tag??"");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var fileName = Path.Combine(path, Name + ".cs");
        // 不覆盖
        if (!cover && File.Exists(fileName))
        {
            Console.WriteLine("skip dto file:" + fileName);
            return;
        }
        File.WriteAllText(fileName, ToString());
        Console.WriteLine("Created dto file:" + fileName);
    }
}
