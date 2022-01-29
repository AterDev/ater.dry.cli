

using System.ComponentModel.DataAnnotations;

namespace CodeGenerator.Test.Base;

/// <summary>
/// 数据加基础字段模型
/// </summary>
/// <inheritdoc/>
public class BaseDB
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// 状态
    /// </summary>
    public virtual Status Status { get; set; } = Status.Default;
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
}

public enum Status
{
    /// <summary>
    /// 默认值 
    /// </summary>
    Default,
    /// <summary>
    /// 已删除
    /// </summary>
    Deleted,
    /// <summary>
    /// 无效
    /// </summary>
    Invalid,
    Valid
}