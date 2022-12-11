using System.ComponentModel.DataAnnotations;

namespace Datastore.Entity;

public class EntityBase
{
    [Key]
    public int Id { get; set; }
}
