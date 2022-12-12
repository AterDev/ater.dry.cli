using System.ComponentModel.DataAnnotations;

namespace Core.Models;

public class EntityBase
{
    [Key]
    public int Id { get; set; }

    [MaxLength(60)]
    public required string ProjectId { get; set; }
}
