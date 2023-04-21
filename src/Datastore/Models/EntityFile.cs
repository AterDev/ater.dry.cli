namespace Datastore.Models;

public class EntityFile
{
    public required string Name { get; set; }
    /// <summary>
    /// 注释说明
    /// </summary>
    public string? Comment { get; set; }
    public required string BaseDirPath { get; set; }
    public required string Path { get; set; }
    public string? Content { get; set; }

    public bool HasDto { get; set; } = false;
    public bool HasManager { get; set; } = false;
    public bool HasAPI { get; set; } = false;
}
