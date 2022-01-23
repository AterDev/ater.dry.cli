namespace CommandLine.Test.Models.BlogDtos;

public class BlogFilter : FilterBase
{
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string? Name { get; set; }
    public Guid? OneCommentId { get; set; }
    public Guid? Comments2Id { get; set; }
    
}
