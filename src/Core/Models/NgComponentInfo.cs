namespace Core.Models;
public class NgComponentInfo(string name)
{
    public string Name { get; set; } = name;
    public string? Routing { get; set; }
    public string? TsContent { get; set; }
    public string? HtmlContent { get; set; }
    public string? CssContent { get; set; }
    public string? ModuleName { get; set; }
}
