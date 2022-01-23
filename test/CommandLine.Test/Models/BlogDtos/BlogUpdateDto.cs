namespace CommandLine.Test.Models.BlogDtos;

public class BlogUpdateDto
{
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string? Name { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
    public int? Age2 { get; set; }
    public DateOnly? DateOnly { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public Status? Status { get; set; }
    public Guid? OneCommentId { get; set; }
    
}
