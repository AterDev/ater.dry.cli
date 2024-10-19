using Entity;
namespace Share.Models.GenActionDtos;
/// <summary>
/// The project's generate action更新时DTO
/// </summary>
/// <see cref="Entity.GenAction"/>
public class GenActionUpdateDto
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    public string? Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    /// <summary>
    /// 实体路径
    /// </summary>
    public string? EntityPath { get; set; }
    /// <summary>
    /// open api path
    /// </summary>
    public string? OpenApiPath { get; set; }
    public List<Variable>? Variables { get; set; }
    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType? SourceType { get; set; }
    public Guid? ProjectId { get; set; }
    public List<Guid>? GenStepIds { get; set; }
    
}
