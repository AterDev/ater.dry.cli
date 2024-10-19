namespace Share.Models;
/// <summary>
/// 属性差异
/// </summary>
public class DiffProperties
{
    /// <summary>
    /// 对比的模型
    /// </summary>
    public string? ModelName { get; set; }
    /// <summary>
    /// 新增的属性
    /// </summary>
    public List<PropertyInfo> Added { get; set; } = [];
    /// <summary>
    /// 删除的属性
    /// </summary>
    public List<PropertyInfo> Deleted { get; set; } = [];
}
