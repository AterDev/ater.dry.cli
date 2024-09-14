namespace Share.Models;

public class EntityFile
{
    public required string Name { get; set; }
    /// <summary>
    /// 注释说明
    /// </summary>
    public string? Comment { get; set; }
    public string BaseDirPath { get; set; } = string.Empty;
    public required string FullName { get; set; }
    public string? Content { get; set; }
    /// <summary>
    /// 所属模块
    /// </summary>
    public string? ModuleName { get; set; }

    public bool HasDto { get; set; }
    public bool HasManager { get; set; }
    public bool HasAPI { get; set; }

    /// <summary>
    /// 获取manager路径
    /// </summary>
    /// <returns></returns>
    public string GetManagerPath(string basePath)
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(basePath, Const.ManagersDir)
            : Path.Combine(basePath, PathConst.ModulesPath, ModuleName, Const.ManagersDir);
    }

    public string GetDtoPath(string basePath)
    {
        var name = Path.GetFileNameWithoutExtension(Name);
        return ModuleName.IsEmpty()
            ? Path.Combine(basePath, Const.ModelsDir, $"{name}Dtos")
            : Path.Combine(basePath, PathConst.ModulesPath, ModuleName, Const.ModelsDir, $"{name}Dtos");
    }
    public string GetControllerPath(string basePath)
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(basePath, Const.ControllersDir)
            : Path.Combine(basePath, PathConst.ModulesPath, ModuleName, Const.ControllersDir);
    }
}
