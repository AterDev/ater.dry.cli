using CodeGenerator;
using Humanizer;
using Share.Infrastructure.Helper;

namespace Application.Services;
/// <summary>
/// 解决方案相关功能
/// </summary>
public class SolutionService(IProjectContext projectContext, ILogger<SolutionService> logger)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly ILogger<SolutionService> _logger = logger;
    public string SolutionPath { get; set; } = projectContext.SolutionPath ?? string.Empty;

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="ModuleName"></param>
    public async Task CreateModuleAsync(string moduleName)
    {
        string moduleDir = Path.Combine(SolutionPath, PathConst.ModulesPath);
        if (!Directory.Exists(moduleDir))
        {
            Directory.CreateDirectory(moduleDir);
        }
        if (Directory.Exists(Path.Combine(moduleDir, moduleName)))
        {
            _logger.LogInformation("⚠️ 该模块已存在");
            return;
        }
        // 基础类
        string projectPath = Path.Combine(moduleDir, moduleName);
        await Console.Out.WriteLineAsync($"🚀 create module:{moduleName} ➡️ {projectPath}");

        // global usings
        string usingsContent = TplContent.ModuleGlobalUsings(moduleName);
        usingsContent = usingsContent.Replace("${Module}", moduleName);
        await AssemblyHelper.GenerateFileAsync(projectPath, Const.GlobalUsingsFile, usingsContent, true);

        // project file
        string targetVersion = Const.Version;
        var csprojFiles = Directory.GetFiles(_projectContext.ApiPath!, $"*{Const.CSharpProjectExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (csprojFiles != null)
        {
            targetVersion = AssemblyHelper.GetTargetFramework(csprojFiles) ?? Const.Version;
        }
        string csprojContent = TplContent.DefaultModuleCSProject(targetVersion);
        await AssemblyHelper.GenerateFileAsync(projectPath, $"{moduleName}{Const.CSharpProjectExtension}", csprojContent);

        // create dirs
        Directory.CreateDirectory(Path.Combine(projectPath, Const.ModelsDir));
        Directory.CreateDirectory(Path.Combine(projectPath, Const.ManagersDir));
        Directory.CreateDirectory(Path.Combine(projectPath, Const.ControllersDir));
        // 模块文件
        await AssemblyHelper.GenerateFileAsync(projectPath, "InitModule.cs", GetInitModuleContent(moduleName));
        await AssemblyHelper.GenerateFileAsync(projectPath, Const.ServiceExtensionsFile, TplContent.ModuleServiceCollection(moduleName));

        await AddDefaultModuleAsync(moduleName);
        await AddModuleConstFieldAsync(moduleName);
        // update solution file
        UpdateSolutionFile(Path.Combine(projectPath, $"{moduleName}{Const.CSharpProjectExtension}"));
    }

    /// <summary>
    /// 添加模块常量
    /// </summary>
    public async Task AddModuleConstFieldAsync(string moduleName)
    {
        string moduleConstPath = Path.Combine(_projectContext.EntityPath!, "Modules.cs");
        if (File.Exists(moduleConstPath))
        {
            var entityPath = _projectContext.Project!.Config.IsLight
                ? PathConst.DefinitionPath
                : _projectContext.EntityPath;
            if (entityPath != null)
            {
                CompilationHelper analyzer = new(entityPath);
                string content = File.ReadAllText(moduleConstPath);
                analyzer.LoadContent(content);
                string fieldName = moduleName.Replace("Mod", "");

                if (!analyzer.FieldExist(fieldName))
                {
                    string newField = @$"public const string {fieldName} = ""{moduleName}"";";
                    analyzer.AddClassField(newField);
                    content = analyzer.SyntaxRoot!.ToFullString();
                    await AssemblyHelper.GenerateFileAsync(moduleConstPath, content, true);
                }
            }
        }
    }

    public void CleanModule(string moduleName)
    {
        var entityPath = Path.Combine(_projectContext.EntityPath!, moduleName);
        var entityFrameworkPath = _projectContext.EntityFrameworkPath!;

        if (Directory.Exists(entityPath))
        {
            Directory.Delete(entityPath, true);
            // 从解决方案移除项目
            var modulePath = Path.Combine(_projectContext.ModulesPath!, moduleName + "Mod");
            var moduleProjectFilePath = Path.Combine(modulePath, $"{moduleName}Mod{Const.CSharpProjectExtension}");
            ProcessHelper.RunCommand("dotnet", $"sln {SolutionPath} remove {moduleProjectFilePath}", out string error);
            Directory.Delete(modulePath, true);
        }
    }

    /// <summary>
    /// 获取模块列表
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <returns>file path</returns>
    public static List<string>? GetModulesPaths(string solutionPath)
    {
        string modulesPath = Path.Combine(solutionPath, "src", "Modules");
        if (!Directory.Exists(modulesPath))
        {
            return default;
        }
        List<string> files = [.. Directory.GetFiles(modulesPath, $"*{Const.CSharpProjectExtension}", SearchOption.AllDirectories)];
        return files.Count != 0 ? files : default;
    }

    private static string GetInitModuleContent(string moduleName)
    {
        return $$"""
            using Microsoft.Extensions.Configuration;
            namespace {{moduleName}};
            public class InitModule
            {
                /// <summary>
                /// 模块初始化方法
                /// </summary>
                /// <param name="provider"></param>
                /// <returns></returns>
                public static async Task InitializeAsync(IServiceProvider provider)
                {
                    ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                    CommandDbContext context = provider.GetRequiredService<CommandDbContext>();
                    ILogger<InitModule> logger = loggerFactory.CreateLogger<InitModule>();
                    IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                    try
                    {
                       // TODO:初始化逻辑
                        await Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("初始化{{moduleName}}失败！{message}", ex.Message);
                    }
                }
            }
            """;
    }

    /// <summary>
    /// 使用 dotnet sln add
    /// </summary>
    /// <param name="projectPath"></param>
    private void UpdateSolutionFile(string projectPath)
    {
        FileInfo? slnFile = AssemblyHelper.GetSlnFile(new DirectoryInfo(SolutionPath));
        if (slnFile != null)
        {
            // 添加到解决方案
            if (!ProcessHelper.RunCommand("dotnet", $"sln {slnFile.FullName} add {projectPath}", out string error))
            {
                _logger.LogInformation("add project ➡️ solution failed:" + error);
            }
            else
            {
                _logger.LogInformation("✅ add project ➡️ solution!");
            }
        }
        var csprojFiles = Directory.GetFiles(_projectContext.ApiPath!, $"*{Const.CSharpProjectExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (File.Exists(csprojFiles))
        {
            // 添加到主服务
            if (!ProcessHelper.RunCommand("dotnet", $"add {csprojFiles} reference {projectPath}", out string error))
            {
                _logger.LogInformation("add project reference failed:" + error);
            }
        }
    }

    /// <summary>
    /// 添加默认模块
    /// </summary>
    private async Task AddDefaultModuleAsync(string moduleName)
    {
        var moduleNames = ModuleInfo.GetModules().Select(m => m.Value).ToList();
        if (!moduleNames.Contains(moduleName))
        {
            return;
        }
        string studioPath = AssemblyHelper.GetStudioPath();
        string sourcePath = Path.Combine(studioPath, "Modules", moduleName);
        if (!Directory.Exists(sourcePath))
        {
            _logger.LogInformation($"🦘 no default {moduleName}, just init it!");
            return;
        }

        string databasePath = _projectContext.EntityFrameworkPath!;
        string entityPath = Path.Combine(_projectContext.EntityPath!, moduleName);
        string modulePath = Path.Combine(SolutionPath, PathConst.ModulesPath, moduleName);

        _logger.LogInformation("🚀 copy module files");
        // copy entities
        CopyModuleFiles(Path.Combine(sourcePath, "Entity"), entityPath);

        // copy module files
        CopyModuleFiles(sourcePath, modulePath);

        _logger.LogInformation("🚀 update ContextBase DbSet");
        string dbContextFile = Path.Combine(databasePath, "DBProvider", "ContextBase.cs");
        string dbContextContent = File.ReadAllText(dbContextFile);

        CompilationHelper compilation = new(databasePath);
        compilation.LoadContent(dbContextContent);

        List<FileInfo> entityFiles = new DirectoryInfo(Path.Combine(sourcePath, "Entity"))
            .GetFiles("*.cs", SearchOption.AllDirectories)
            .ToList();

        entityFiles.ForEach(file =>
        {
            string entityName = Path.GetFileNameWithoutExtension(file.Name);
            var plural = entityName.Pluralize();
            string propertyString = $@"public DbSet<{entityName}> {plural} {{ get; set; }}";

            if (!compilation.PropertyExist(plural))
            {
                compilation.AddClassProperty(propertyString);
                _logger.LogInformation($"  ℹ️ add new property {plural} ➡️ ContextBase");
            }
        });

        dbContextContent = compilation.SyntaxRoot!.ToFullString();
        File.WriteAllText(dbContextFile, dbContextContent);
        // update globalUsings.cs
        string globalUsingsFile = Path.Combine(databasePath, "GlobalUsings.cs");
        string globalUsingsContent = File.ReadAllText(globalUsingsFile);

        string newLine = @$"global using Entity.{moduleName};";
        if (!globalUsingsContent.Contains(newLine))
        {
            _logger.LogInformation($"  ℹ️ add new using {newLine} ➡️ GlobalUsings");
            globalUsingsContent = globalUsingsContent.Replace("global using Entity;", $"global using Entity;{Environment.NewLine}{newLine}");
            File.WriteAllText(globalUsingsFile, globalUsingsContent);
        }

        // 重新生成依赖注入服务
        string applicationPath = _projectContext.ApplicationPath!;
        string entityFrameworkPath = _projectContext.EntityFrameworkPath!;

        string content = ManagerGenerate.GetManagerServiceContent(applicationPath, moduleName);
        await IOHelper.WriteToFileAsync(Path.Combine(applicationPath, "ManagerServiceCollectionExtensions.cs"), content);
    }

    /// <summary>
    /// 复制模块文件
    /// </summary>
    /// <param name="sourceDir"></param>`
    /// <param name="destinationDir"></param>
    /// <param name="recursive"></param>
    private void CopyModuleFiles(string sourceDir, string destinationDir)
    {
        DirectoryInfo dir = new(sourceDir);
        if (!dir.Exists) { return; }

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        // 获取源目录中的文件并复制到目标目录
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
            _logger.LogInformation($"  ℹ️ copy {file.Name} ➡️ {targetFilePath}");
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            // 过滤不必要的目录
            if (subDir.Name is "Entity" or "Application")
            {
                continue;
            }
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyModuleFiles(subDir.FullName, newDestinationDir);
        }
    }
}
