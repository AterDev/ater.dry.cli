using System.ComponentModel.DataAnnotations;

namespace AterStudio.Shared.Models;

public record AddProjectForm
{
  [Required]
  public required string DisplayName { get; set; }
  [Required]
  public required string Path { get; set; }
}
