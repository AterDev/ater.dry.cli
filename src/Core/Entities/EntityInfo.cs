using LiteDB;
using PropertyInfo = Core.Models.PropertyInfo;

namespace Core.Entities;
/// <summary>
/// defined entity model 
/// </summary>
public class EntityInfo : EntityBase
{
    /// <summary>
    /// 类名
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// 命名空间
    /// </summary>
    public string? NamespaceName { get; set; }
    /// <summary>
    /// 程序集名称
    /// </summary>
    public string? AssemblyName { get; set; }
    /// <summary>
    /// 类注释
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 类注释
    /// </summary>
    public string? Summary { get; set; }
    public EntityKeyType KeyType { get; set; } = EntityKeyType.Guid;

    /// <summary>
    /// 是否为枚举类
    /// </summary>
    public bool? IsEnum { get; set; } = false;
    public bool IsList { get; set; }

    /// <summary>
    /// 属性
    /// </summary>
    public IReadOnlyList<PropertyInfo> PropertyInfos { get; set; } = new List<PropertyInfo>();

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
