using System.ComponentModel.DataAnnotations;

namespace CodeGenerator.Test.Entity;

/// <summary>
/// 博客
/// </summary>
public class Blog : BaseDB
{
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public Blog? Parent { get; set; }
    public List<Blog>? Children { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
    public List<Comments>? Comments { get; set; }
    public DateOnly DateOnly { get; set; }
}

