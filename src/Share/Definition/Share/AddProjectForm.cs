namespace Definition.Share;

public record AddProjectForm
{
    public required string DisplayName { get; set; }
    public required string Path { get; set; }
}
