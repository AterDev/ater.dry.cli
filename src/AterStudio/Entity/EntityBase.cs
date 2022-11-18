using System.ComponentModel.DataAnnotations;

namespace AterStudio.Entity;

public class EntityBase
{
    [Key]
    public int Id { get; set; }
}
