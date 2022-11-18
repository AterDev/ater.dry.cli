using System.ComponentModel.DataAnnotations;

namespace Studio.Entity;

public class EntityBase
{
    [Key]
    public int Id { get; set; }
}
