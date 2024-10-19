using Entity;
namespace Share.Models.GenActionDtos;
/// <summary>
/// The project's generate action筛选条件
/// </summary>
/// <see cref="Entity.GenAction"/>
public class GenActionFilterDto : FilterBase
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    public string? Name { get; set; }
    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType? SourceType { get; set; }
    public Guid? ProjectId { get; set; }
    
}
