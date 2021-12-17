using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Models;

public class TestEntity
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string? Name { get; set; }
    List<Comments>? Comments { get; set; }
}

[EntityTypeConfiguration(typeof(CommentEntityConfiguration))]
public class Comments
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
    public string? Content { get; set; }
    public TestEntity TestEntity { get; set; }
    public Status Status { get; set; }
}
public enum Status
{
    Default,
    New,
    Deleted
}