namespace CodeGenerator.Models;
public class NgComponentInfo
{
    public NgComponentInfo(string name)
    {
        Name = name;
    }
    public string Name { get; set; } = default!;
    public string? Routing { get; set; }
    public string? TsContent { get; set; }
    public string? HtmlContent { get; set; }
    public string? CssContent { get; set; }
    public string? ModuleName { get; set; }
}
