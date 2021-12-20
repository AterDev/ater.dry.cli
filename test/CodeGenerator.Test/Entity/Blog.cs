using System.ComponentModel.DataAnnotations;

namespace CodeGenerator.Test.Entity;

public class Blog
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string? Name { get; set; }
    public string? Content { get; set; }
    List<Comments>? Comments { get; set; }
}


public enum Status
{
    Default,
    New,
    Deleted
}