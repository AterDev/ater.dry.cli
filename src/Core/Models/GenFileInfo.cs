namespace Core.Models;

/// <summary>
/// 生成文件的信息，主要用来存储多个文件生成的内容
/// </summary>
public class GenFileInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; } = default!;
    public string Content { get; set; }
    /// <summary>
    /// 相对文件夹名称
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// 模型名称
    /// </summary>
    public string ModelName { get; set; }

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
