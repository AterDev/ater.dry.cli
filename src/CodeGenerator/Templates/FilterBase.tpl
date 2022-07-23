namespace ${Namespace}.Models;
public partial class FilterBase
{
    public int? PageIndex { get; set; } = 1;
    public int? PageSize { get; set; } = 12;
    /// <summary>
    /// 排序
    /// </summary>
    public Dictionary<string, bool>? OrderBy { get; set; }
}
