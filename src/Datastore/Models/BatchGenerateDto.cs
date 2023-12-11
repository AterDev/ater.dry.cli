namespace Datastore.Models;

public class BatchGenerateDto
{
    public required Guid ProjectId { get; set; }
    public required List<string> EntityPaths { get; set; }
    public CommandType CommandType { get; set; }
    /// <summary>
    /// 项目路径
    /// </summary>
    public List<string>? ProjectPath { get; set; }
    public bool Force { get; set; }
}
