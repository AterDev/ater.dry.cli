using System.ComponentModel.DataAnnotations;

namespace Definition.Entity;
/// <summary>
/// 项目
/// </summary>
public class Project : EntityBase
{
    /// <summary>
    /// 项目名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }
    /// <summary>
    /// 显示名
    /// </summary>
    [MaxLength(100)]
    public required string DisplayName { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(200)]
    public required string Path { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [MaxLength(20)]
    public string? Version { get; set; }
    /// <summary>
    /// 解决方案类型
    /// </summary>
    public SolutionType? SolutionType { get; set; }

    /// <summary>
    /// Front Path
    /// </summary>
    [MaxLength(200)]
    public string? FrontPath { get; set; }

    public List<EntityInfo> EntityInfos { get; set; } = [];
    public List<ApiDocInfo> ApiDocInfos { get; set; } = [];
    public List<TemplateFile> TemplateFiles { get; set; } = [];
}

public enum SolutionType
{
    DotNet,
    Node
}
