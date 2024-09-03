namespace Share.Models.EntityInfoDtos;
/// <summary>
/// 实体添加时请求结构
/// </summary>
/// <see cref="EntityInfo"/>
public class EntityInfoAddDto
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
    public bool? IsEnum { get; set; }
    public bool IsList { get; set; }
    public required Guid ProjectId { get; set; } = default!;

}
