using Ater.Web.Core.Utils;

namespace Entity;
/// <summary>
/// 实体
/// </summary>
[Index(nameof(Name))]
public class EntityInfo : EntityBase
{
    public static string[] IgnoreTypes = ["JsonDocument?", "byte[]"];
    public static string[] IgnoreProperties = [
        Const.Id,
        Const.CreatedTime,
        Const.UpdatedTime,
        Const.IsDeleted,
        "PageSize", "PageIndex"
        ];

    /// <summary>
    /// file content md5 hash
    /// </summary>
    [MaxLength(32)]
    public required string Md5Hash { get; set; }

    /// <summary>
    /// module name
    /// </summary>
    public string? ModuleName { get; set; }

    /// <summary>
    /// file path
    /// </summary>
    [MaxLength(200)]
    public required string FilePath { get; set; }
    /// <summary>
    /// 类名
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }
    /// <summary>
    /// 命名空间
    /// </summary>
    [MaxLength(100)]
    public required string NamespaceName { get; set; }
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
    /// 获取manager路径
    /// </summary>
    /// <returns></returns>
    public string GetManagerPath()
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(Project.Path, PathConst.ApplicationPath, Const.ManagersDir)
            : Path.Combine(Project.Path, PathConst.ModulesPath, ModuleName, Const.ManagersDir);
    }

    public string GetDtoPath()
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(Project.Path, PathConst.SharePath, Const.ModelsDir, $"{Name}Dtos")
            : Path.Combine(Project.Path, PathConst.ModulesPath, ModuleName, Const.ModelsDir, $"{Name}Dtos");
    }
    public string GetControllerPath()
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(Project.Path, PathConst.APIPath, Const.ControllersDir)
            : Path.Combine(Project.Path, PathConst.ModulesPath, ModuleName, Const.ControllersDir);
    }

    public string GetDtoNamespace()
    {
        return GetShareNamespace();
    }

    public string GetShareNamespace()
    {
        return ModuleName.IsEmpty()
            ? Const.ShareName
            : ModuleName;
    }

    public string GetManagerNamespace()
    {
        return ModuleName.IsEmpty()
            ? Const.ApplicationName
            : ModuleName;
    }

    public string GetAPINamespace()
    {
        return ModuleName.IsEmpty()
            ? Const.APIName
            : ModuleName;
    }

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

    /// <summary>
    /// 获取筛选属性
    /// </summary>
    /// <returns></returns>
    public List<PropertyInfo> GetFilterProperties()
    {
        return PropertyInfos
            .Where(p => p.IsRequired && !p.IsNavigation
                    || !p.IsList
                        && !p.IsNavigation
                        && !p.IsComplexType
                        && !IgnoreProperties.Contains(p.Name)
                        && !IgnoreTypes.Contains(p.Type)
                    || p.IsEnum
                    )
                .Where(p => p.MaxLength is not (not null and >= 100))
            .ToList() ?? [];
    }
}
public enum EntityKeyType
{
    Guid,
    Int,
    String
}
