namespace Share.Models.GenActionDtos;
/// <summary>
/// The project's generate action添加时DTO
/// </summary>
/// <see cref="Entity.GenAction"/>
public class GenActionAddDto
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    public required string Name { get; set; }
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
    public List<Variable> Variables { get; set; } = [];
    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType? SourceType { get; set; }
}
