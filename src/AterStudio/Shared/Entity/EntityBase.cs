using System.ComponentModel.DataAnnotations;

namespace AterStudio.Shared.Entity;

public class EntityBase
{
  [Key]
  public int Id { get; set; }
}
