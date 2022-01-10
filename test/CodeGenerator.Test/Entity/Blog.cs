using System.ComponentModel.DataAnnotations;

namespace CodeGenerator.Test.Entity;

public class Blog : BaseDB
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    public string? Name { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
    List<Comments>? Comments { get; set; }
    IEnumerable<string> Test1 { get; set; }
    public Comments OneComment { get; set; }
    public Comments? Comments2 { get; set; }
    public Nullable<int> Age { get; set; }
    public int? Age2 { get; set; }
    ICollection<string> Test2 { get; set; }
    public Status Status { get; set; }
    public DateOnly DateOnly { get; set; }
}


public enum Status
{
    Default,
    New,
    Deleted
}