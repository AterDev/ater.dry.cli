using LiteDB;

namespace Core.Models;
/// <summary>
/// defined entity model 
/// </summary>
public class EntityInfo : EntityBase
{
    /// <summary>
    /// 类名
    /// </summary>
    public string Name { get; set; }
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
    public EntityKeyType KeyType { get; set; } = EntityKeyType.Guid;

    /// <summary>
    /// 属性
    /// </summary>
    [BsonRef]
    public List<PropertyInfo> PropertyInfos { get; set; } = new List<PropertyInfo>();

    /// <summary>
    /// 获取导航属性
    /// </summary>
    /// <returns></returns>
    public PropertyInfo? GetNavigation()
    {
        return PropertyInfos?.Where(p => p.IsNavigation
            && p.HasMany == false
            && p.IsNullable == false)
            .FirstOrDefault();
    }
}
public enum EntityKeyType
{
    Guid,
    Int,
    String
}
