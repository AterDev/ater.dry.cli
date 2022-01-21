namespace CodeGenerator.Models;

public class GenFileInfo
{
    public string? Name { get; set; }
    public string Content { get; set; }
    public string? Path { get; set; }

    public GenFileInfo(string content)
    {
        Content = content;
    }
    public GenFileInfo(string name, string content)
    {
        Name = name;
        Content = content;
    }
}
