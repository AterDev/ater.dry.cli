using System.ComponentModel.DataAnnotations;

namespace Definition.Entity;
/// <summary>
/// 模板内容
/// </summary>
public class TemplateFile : EntityBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [MaxLength(60)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(10_000)]
    public string? Content { get; set; }

    public Project Project { get; set; } = null!;
    public Guid ProjectId { get; set; } = default!;

}
