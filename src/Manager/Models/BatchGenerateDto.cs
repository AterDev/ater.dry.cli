namespace Manager.Models;

public class BatchGenerateDto
{
    public required int ProjectId { get; set; }
    public required List<string> EntityPaths { get; set; }
    public CommandType CommandType { get; set; }
}
