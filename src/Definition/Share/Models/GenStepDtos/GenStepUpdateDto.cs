namespace Share.Models.GenStepDtos;
/// <summary>
/// task step更新时DTO
/// </summary>
/// <see cref="Entity.GenStep"/>
public class GenStepUpdateDto
{
    /// <summary>
    /// 模板或命令内容
    /// </summary>
    [MaxLength(100_000)]
    public string? Content { get; set; }
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
    /// step type
    /// </summary>
    public GenStepType? GenStepType { get; set; }
}
