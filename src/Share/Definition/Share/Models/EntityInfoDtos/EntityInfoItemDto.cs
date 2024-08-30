using Definition.Entity;
namespace Share.Share.Models.EntityInfoDtos;
/// <summary>
/// 实体列表元素
/// </summary>
/// <see cref="Definition.Entity.EntityInfo"/>
public class EntityInfoItemDto
{
    /// <summary>
    /// 类名
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
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
    public bool? IsEnum { get; set; }
    public bool IsList { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedTime { get; set; }

}
