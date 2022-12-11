using System.ComponentModel.DataAnnotations;

namespace Manager.Entity;

public class EntityBase
{
    [Key]
    public int Id { get; set; }
}
