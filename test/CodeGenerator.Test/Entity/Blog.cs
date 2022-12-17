using System.ComponentModel.DataAnnotations;

namespace CodeGenerator.Test.Entity;

/// <summary>
/// 博客
/// </summary>
public class Blog : BaseDB
{
    /// <summary>
    /// 名称
    /// 博客
    /// </summary>
    [StringLength(100)]
    [MinLength(10)]
    public required string Name { get; set; }
    public required string Title { get; set; }
    [MaxLength(200)]
    public string? Summary { get; set; }
    public Comments? Comment { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(2000)]
    public string? Content { get; set; }
    public List<Comments>? Comments { get; set; }
    public DateOnly DateOnly { get; set; }
}
