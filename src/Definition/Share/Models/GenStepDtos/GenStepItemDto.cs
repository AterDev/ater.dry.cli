namespace Share.Models.GenStepDtos;
/// <summary>
/// task step列表元素
/// </summary>
/// <see cref="Entity.GenStep"/>
public class GenStepItemDto
{
    /// <summary>
    /// 模板或命令内容
    /// </summary>
    [MaxLength(100_000)]
    public string? Content { get; set; }

    /// <summary>
    /// 生成内容
    /// </summary>
    [MaxLength(100_000)]
    public required string OutputContent { get; set; }

    /// <summary>
    /// 模板或脚本路径
    /// </summary>
    [MaxLength(400)]
    public string? Path { get; set; }

    /// <summary>
    /// 输出路径
    /// </summary>
    [MaxLength(400)]
    public string? OutputPath { get; set; }

    /// <summary>
    /// command content
    /// </summary>
    [MaxLength(2000)]
    public string? Command { get; set; }
    /// <summary>
    /// step type
    /// </summary>
    public GenStepType GenStepType { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
