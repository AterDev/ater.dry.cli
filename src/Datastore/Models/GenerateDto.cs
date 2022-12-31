namespace Datastore.Models;

public class GenerateDto
{
    public required Guid ProjectId { get; set; }
    public required string EntityPath { get; set; }
    public CommandType CommandType { get; set; }
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
    API
}
