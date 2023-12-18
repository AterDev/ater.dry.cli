namespace Datastore.Models;

public class GenerateDto
{
    public required Guid ProjectId { get; set; }
    public required string EntityPath { get; set; }
    /// <summary>
    /// 服务
    /// </summary>
    public string? ServiceName { get; set; }
    public CommandType CommandType { get; set; }
    /// <summary>
    /// 是否覆盖
    /// </summary>
    public bool Force { get; set; }
}

/// <summary>
/// 命令类型
/// </summary>
public enum CommandType
{
    /// <summary>
    /// dto
    /// </summary>
    Dto,
    /// <summary>
    /// manager
    /// </summary>
    Manager,
    /// <summary>
    /// api
    /// </summary>
    API,
    /// <summary>
    /// protobuf
    /// </summary>
    Protobuf,
    /// <summary>
    /// 清除
    /// </summary>
    Clear
}
