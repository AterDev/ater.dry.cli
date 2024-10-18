using Entity;
namespace Share.Models.GenActionDtos;
/// <summary>
/// The project's generate action详情
/// </summary>
/// <see cref="Entity.GenAction"/>
public class GenActionDetailDto
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
public string Name { get; set; } = default!;
    [MaxLength(200)]
public string? Description { get; set; }
    /// <summary>
    /// source type
    /// </summary>
public GenSourceType SourceType { get; set; }
public Project Project { get; set; } = default!;
public Guid ProjectId { get; set; } = default!;
public Guid Id { get; set; } = Guid.NewGuid();
public DateTimeOffset CreatedTime { get; set; }
public DateTimeOffset UpdatedTime { get; set; }
    
}
