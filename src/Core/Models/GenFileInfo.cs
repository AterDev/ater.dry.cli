namespace Core.Models;

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
    /// 是否可被用户编辑，如果可编辑，则不能覆盖用户代码
    /// </summary>
    public bool CanModify { get; set; }
    /// <summary>
    /// 相对文件夹名称
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }
}
