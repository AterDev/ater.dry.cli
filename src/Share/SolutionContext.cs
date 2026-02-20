using Microsoft.Extensions.DependencyInjection;

namespace Share;

/// <summary>
/// 项目上下文
/// </summary>
public class SolutionContext(IServiceProvider serviceProvider)
{
    public int? SolutionId { get; set; }
    public string? ProjectName { get; set; }
    public string? SolutionPath { get; set; }
    public string? SharePath { get; set; }
    public string? CommonModPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }
    public string? EntityFrameworkPath { get; set; }
    public string? ModulesPath { get; set; }
    public string? ServicesPath { get; set; }

    public SolutionConfig? SolutionConfig { get; set; }

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private DefaultDbContext? _context;

    private DefaultDbContext DbContext =>
        _context ??= _serviceProvider.GetRequiredService<DefaultDbContext>();

    /// <summary>
    /// 根据ID设置项目
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task SetProjectByIdAsync(int id)
    {
        SolutionId = id;
        var intId = id.GetHashCode();
        var solution = DbContext.Solutions.FirstOrDefault(s => s.Id == intId);
        if (solution != null)
        {
            ProjectName = solution.Name;
            SolutionPath = solution.Path;
            SolutionConfig = solution.Config;
            SharePath = Path.Combine(SolutionPath, SolutionConfig.SharePath);
            CommonModPath = Path.Combine(SolutionPath, SolutionConfig.CommonModPath);
            EntityPath = Path.Combine(SolutionPath, SolutionConfig.EntityPath);
            ApiPath = Path.Combine(SolutionPath, SolutionConfig.ApiPath);
            EntityFrameworkPath = Path.Combine(SolutionPath, SolutionConfig.EntityFrameworkPath);
            ModulesPath = Path.Combine(SolutionPath, PathConst.ModulesPath);
            ServicesPath = Path.Combine(SolutionPath, PathConst.ServicesPath);
        }
        return Task.CompletedTask;
    }

    public Task SetSolutionAsync(string solutionPath)
    {
        if (solutionPath.IndexOf("/") > 0)
        {
            solutionPath = solutionPath.Replace("/", "\\");
        }

        SolutionPath = solutionPath;
        var solution = DbContext
            .Solutions.FirstOrDefault(p => p.Path.Equals(solutionPath));

        SolutionId = solution?.Id;
        SolutionConfig = solution?.Config;
        SharePath = Path.Combine(SolutionPath, SolutionConfig?.SharePath ?? PathConst.SharePath);
        CommonModPath = Path.Combine(
            SolutionPath,
            SolutionConfig?.CommonModPath ?? PathConst.CommonModPath
        );
        EntityPath = Path.Combine(SolutionPath, SolutionConfig?.EntityPath ?? PathConst.EntityPath);
        ApiPath = Path.Combine(SolutionPath, SolutionConfig?.ApiPath ?? PathConst.APIPath);
        EntityFrameworkPath = Path.Combine(
            SolutionPath,
            SolutionConfig?.EntityFrameworkPath ?? PathConst.EntityFrameworkPath
        );
        ModulesPath = Path.Combine(SolutionPath, PathConst.ModulesPath);
        ServicesPath = Path.Combine(SolutionPath, PathConst.ServicesPath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// get share(dto) path
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public string GetDtoPath(string entityName, string? moduleName = null)
    {
        var name = Path.GetFileNameWithoutExtension(entityName);
        return moduleName.IsEmpty()
            ? Path.Combine(SharePath ?? PathConst.SharePath, ConstVal.ModelsDir, $"{name}Dtos")
            : Path.Combine(
                ModulesPath ?? PathConst.ModulesPath,
                moduleName,
                ConstVal.ModelsDir,
                $"{name}Dtos"
            );
    }

    public string GetModulePath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(CommonModPath ?? PathConst.CommonModPath)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName);
    }

    /// <summary>
    /// 获取manager路径
    /// </summary>
    /// <returns></returns>
    public string GetManagerPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(CommonModPath ?? PathConst.CommonModPath, ConstVal.ManagersDir)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName, ConstVal.ManagersDir);
    }

    /// <summary>
    /// controller TemplatePath
    /// </summary>
    /// <returns></returns>
    public string GetControllerPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(
                ModulesPath ?? PathConst.ModulesPath,
                ConstVal.CommonMod,
                ConstVal.ControllersDir
            )
            : Path.Combine(
                ModulesPath ?? PathConst.ModulesPath,
                moduleName,
                ConstVal.ControllersDir
            );
    }

    public string GetApiPath(string? moduleName = null)
    {
        return moduleName.IsEmpty()
            ? Path.Combine(ApiPath ?? PathConst.APIPath)
            : Path.Combine(ModulesPath ?? PathConst.ModulesPath, moduleName);
    }

}
