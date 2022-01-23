namespace CommandLine.Test.Models.BlogDtos;

public class BlogShortDto
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string? Name { get; set; }
    public IEnumerable<string> Test1 { get; set; }
    public Comments OneComment { get; set; }
    public Comments? Comments2 { get; set; }
    public int? Age2 { get; set; }
    public ICollection<string> Test2 { get; set; }
    public DateOnly? DateOnly { get; set; }
    /// <summary>
    /// 状态
    /// </summary>
    public Status? Status { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    
}
