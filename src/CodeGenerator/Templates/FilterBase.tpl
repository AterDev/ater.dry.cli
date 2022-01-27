namespace ${Namespace}.Models;
public partial class FilterBase
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public Guid? TenantId { get; set; }
    public DateTimeOffset? MinCreatedTime { get; set; }
    public DateTimeOffset? MaxCreatedTime { get; set; }
}
