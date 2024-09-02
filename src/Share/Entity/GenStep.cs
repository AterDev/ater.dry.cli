using System.ComponentModel.DataAnnotations.Schema;

namespace Entity;
/// <summary>
/// task step
/// </summary>
public class GenStep : EntityBase
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

    public ICollection<GenAction> GenActions { get; set; } = [];

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;
    public Guid ProjectId { get; set; } = default!;
}

/// <summary>
/// step type
/// </summary>
public enum GenStepType
{
    /// <summary>
    /// generate file
    /// </summary>
    [Description("generate file")]
    File,
    /// <summary>
    /// run command
    /// </summary>
    [Description("run command")]
    Command,
    /// <summary>
    /// run script
    /// </summary>
    [Description("run script")]
    Script
}