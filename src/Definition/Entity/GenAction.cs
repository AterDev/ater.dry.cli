using System.ComponentModel.DataAnnotations.Schema;

namespace Entity;

/// <summary>
/// The project's generate action
/// </summary>
public class GenAction : EntityBase
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType SourceType { get; set; }

    /// <summary>
    /// action step
    /// </summary>
    public ICollection<GenStep> GenSteps { get; set; } = [];

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;
    public Guid ProjectId { get; set; } = default!;
}
/// <summary>
/// Source Type
/// </summary>
public enum GenSourceType
{
    /// <summary>
    /// entity class
    /// </summary>
    [Description("Entity Class")]
    EntityCLass,
    /// <summary>
    /// OpenAPI
    /// </summary>
    [Description("OpenAPI")]
    OpenAPI
}
