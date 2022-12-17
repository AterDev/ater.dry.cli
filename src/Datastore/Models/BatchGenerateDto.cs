namespace Datastore.Models;

public class BatchGenerateDto
{
    public required Guid ProjectId { get; set; }
    public required List<string> EntityPaths { get; set; }
    public CommandType CommandType { get; set; }
}
