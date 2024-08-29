namespace Definition.Share;

public class BatchGenerateDto
{
    public required Guid ProjectId { get; set; }
    public required List<string> EntityPaths { get; set; }

    /// <summary>
    /// 服务
    /// </summary>
    public string? ServiceName { get; set; }
    public CommandType CommandType { get; set; }
    /// <summary>
    /// 项目路径
    /// </summary>
    public List<string>? ProjectPath { get; set; }
    public bool Force { get; set; }
}
