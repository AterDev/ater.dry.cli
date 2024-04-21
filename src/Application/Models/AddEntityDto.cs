namespace Application.Models;

public class AddEntityDto
{
    public string? Namespace { get; set; }

    public required string Content { get; set; }
}
