namespace Datastore.Models;

public class BatchGenerateDto
{
    public required string ProjectId { get; set; }
    public required List<string> EntityPaths { get; set; }
    public CommandType CommandType { get; set; }
}
