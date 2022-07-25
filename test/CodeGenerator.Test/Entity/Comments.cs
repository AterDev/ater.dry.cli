using System.ComponentModel.DataAnnotations;

namespace CodeGenerator.Test.Entity;
[EntityTypeConfiguration(typeof(CommentEntityConfiguration))]
public class Comments
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
    public string? Content { get; set; }
    public Blog? Blog { get; set; }
    public Status Status { get; set; }
}