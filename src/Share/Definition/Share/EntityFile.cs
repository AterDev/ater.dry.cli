namespace Share.Share;

public class EntityFile
{
    public required string Name { get; set; }
    /// <summary>
    /// 注释说明
    /// </summary>
    public string? Comment { get; set; }
    public string BaseDirPath { get; set; } = string.Empty;
    public required string Path { get; set; }
    public string? Content { get; set; }
    /// <summary>
    /// 所属模块
    /// </summary>
    public string? Module { get; set; }

    public bool HasDto { get; set; }
    public bool HasManager { get; set; }
    public bool HasAPI { get; set; }
}
