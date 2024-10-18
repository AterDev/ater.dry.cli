using Entity;
namespace Share.Models.GenStepDtos;
/// <summary>
/// task step详情
/// </summary>
/// <see cref="Entity.GenStep"/>
public class GenStepDetailDto
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
public GenStepType GenStepType { get; set; }
public Project Project { get; set; } = default!;
public Guid ProjectId { get; set; } = default!;
public Guid Id { get; set; } = Guid.NewGuid();
public DateTimeOffset CreatedTime { get; set; }
public DateTimeOffset UpdatedTime { get; set; }
    
}
