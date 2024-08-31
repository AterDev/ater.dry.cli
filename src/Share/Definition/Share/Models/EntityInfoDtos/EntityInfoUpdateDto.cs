namespace Share.Share.Models.EntityInfoDtos;
/// <summary>
/// 实体更新时请求结构
/// </summary>
/// <see cref="Definition.Entity.EntityInfo"/>
public class EntityInfoUpdateDto
{
    /// <summary>
    /// 类名
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
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
    public EntityKeyType? KeyType { get; set; }
    /// <summary>
    /// 是否为枚举类
    /// </summary>
    public bool? IsEnum { get; set; }
    public bool? IsList { get; set; }
    public Guid? ProjectId { get; set; }
    public List<Guid>? PropertyInfoIds { get; set; }

}
