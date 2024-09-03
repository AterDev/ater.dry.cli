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
    public string GetManagerPath(string solutionPath)
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(solutionPath, PathConst.ApplicationPath, Const.ManagersDir)
            : Path.Combine(solutionPath, PathConst.ModulesPath, ModuleName, Const.ManagersDir);
    }

    public string GetDtoPath(string solutionPath)
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(solutionPath, PathConst.SharePath, Const.ModelsDir, $"{Name}Dtos")
            : Path.Combine(solutionPath, PathConst.ModulesPath, ModuleName, Const.ModelsDir, $"{Name}Dtos");
    }
    public string GetControllerPath(string solutionPath)
    {
        return ModuleName.IsEmpty()
            ? Path.Combine(solutionPath, PathConst.APIPath, Const.ControllersDir)
            : Path.Combine(solutionPath, PathConst.ModulesPath, ModuleName, Const.ControllersDir);
    }
}
