namespace Core.Entities;
/// <summary>
/// 项目
/// </summary>
public class Project : EntityBase
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string Path { get; set; }
    public string? Version { get; set; }
    public SolutionType SolutionType { get; set; }
}

public enum SolutionType
{
    DotNet,
    Node
}
