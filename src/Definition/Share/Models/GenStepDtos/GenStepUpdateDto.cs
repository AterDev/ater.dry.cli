using Entity;
namespace Share.Models.GenStepDtos;
/// <summary>
/// task step更新时DTO
/// </summary>
/// <see cref="Entity.GenStep"/>
public class GenStepUpdateDto
{
    /// <summary>
    /// template content
    /// </summary>
    [MaxLength(10_000)]
public string? TemplateContent { get; set; }
    /// <summary>
    /// file outPath or script Path
    /// </summary>
public string? Path { get; set; }
    /// <summary>
    /// command content
    /// </summary>
    [MaxLength(1000)]
public string? Command { get; set; }
    /// <summary>
    /// step type
    /// </summary>
public GenStepType? GenStepType { get; set; }
public Guid? ProjectId { get; set; }
public List<Guid>? GenActionIds { get; set; }
    
}
