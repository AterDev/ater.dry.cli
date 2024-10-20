using System.ComponentModel.DataAnnotations.Schema;

namespace Entity;

/// <summary>
/// 生成操作
/// </summary>
[Index(nameof(Name))]
[Index(nameof(Description))]
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

public class Variable
{
    [MaxLength(100)]
    public required string Key { get; set; }
    [MaxLength(1000)]
    public required string Value { get; set; }
}