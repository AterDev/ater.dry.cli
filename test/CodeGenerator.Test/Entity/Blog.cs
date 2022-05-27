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

    #region region的内容
    public Blog? Parent { get; set; }
    public Comments? Comment { get; set; }

    #endregion

    #region test

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
    public List<Comments>? Comments { get; set; }
    public DateOnly DateOnly { get; set; }
    #endregion

}
