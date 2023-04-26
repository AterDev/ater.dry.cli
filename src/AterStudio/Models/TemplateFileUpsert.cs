using System.ComponentModel.DataAnnotations;

namespace AterStudio.Models;

public class TemplateFileUpsert
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(10_000)]
    public required string Content { get; set; }
}
