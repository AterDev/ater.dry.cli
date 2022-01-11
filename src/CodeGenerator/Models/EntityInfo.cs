namespace CodeGenerator.Models;
/// <summary>
/// defined entity model 
/// </summary>
public class EntityInfo
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
    public List<PropertyInfo>? PropertyInfos { get; set; }

    public EntityInfo(string name)
    {
        Name = name;
    }


    public enum EntityKeyType
    {
        Guid,
        Int,
        String
    }
}
