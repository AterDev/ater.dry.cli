namespace CodeGenerator.Models;

/// <summary>
/// 生成文件的信息，主要用来存储多个文件生成的内容
/// </summary>
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
