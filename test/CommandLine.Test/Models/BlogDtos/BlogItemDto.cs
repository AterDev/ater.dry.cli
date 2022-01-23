namespace CommandLine.Test.Models.BlogDtos;

public class BlogItemDto
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string? Name { get; set; }
    public int? Age2 { get; set; }
    public DateOnly? DateOnly { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public Status? Status { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    
}
