namespace Studio.Models;

public class EntityFile
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public string? Content { get; set; }
}
