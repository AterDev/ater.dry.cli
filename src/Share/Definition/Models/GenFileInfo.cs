namespace Share.Models;

/// <summary>
/// 生成文件的信息，主要用来存储多个文件生成的内容
/// </summary>
public class GenFileInfo(string name, string content)
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; } = name;
    public string Content { get; set; } = content;

    /// <summary>
    /// 是否覆盖
    /// </summary>
    public bool IsCover { get; set; }

    /// <summary>
    /// file path
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }
}
