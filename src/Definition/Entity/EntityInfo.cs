using System.ComponentModel.DataAnnotations;

namespace Definition.Entity;
/// <summary>
/// 实体
/// </summary>
[Index(nameof(Name))]
public class EntityInfo : EntityBase
{
    /// <summary>
    /// 类名
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }
    /// <summary>
    /// 命名空间
    /// </summary>
    [MaxLength(100)]
    public string? NamespaceName { get; set; }
    /// <summary>
    /// 程序集名称
    /// </summary>
    [MaxLength(100)]
    public string? AssemblyName { get; set; }
    /// <summary>
    /// 类注释
    /// </summary>
    [MaxLength(300)]
    public string? Comment { get; set; }
    /// <summary>
    /// 类注释
    /// </summary>
    [MaxLength(100)]
    public string? Summary { get; set; }
    public EntityKeyType KeyType { get; set; } = EntityKeyType.Guid;

    /// <summary>
    /// 是否为枚举类
    /// </summary>
    public bool? IsEnum { get; set; } = false;
    public bool IsList { get; set; }

    public Project Project { get; set; } = null!;
    public Guid ProjectId { get; set; } = default!;

    /// <summary>
    /// 属性
    /// </summary>
    public List<PropertyInfo> PropertyInfos { get; set; } = [];

    /// <summary>
    /// 获取导航属性
    /// </summary>
    /// <returns></returns>
    public List<PropertyInfo>? GetRequiredNavigation()
    {
        return PropertyInfos?.Where(p => p.IsNavigation
            && p.HasMany == false
            && p.IsRequired)
            .ToList();
    }
}
public enum EntityKeyType
{
    Guid,
    Int,
    String
}
