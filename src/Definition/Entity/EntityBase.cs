
using System.ComponentModel.DataAnnotations;

namespace Definition.Entity;

public abstract class EntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

}
