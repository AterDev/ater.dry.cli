namespace Manager.Models;

public record AddProjectForm
{
    public required string DisplayName { get; set; }
    public required string Path { get; set; }
}
