using CodeGenerator;
using CodeGenerator.Helper;

namespace Command.Share.Commands;
/// <summary>
/// 模块命令
/// </summary>
public class ModuleCommand(string solutionPath, List<string> moduleNames)
{
    public List<string> ModuleNames { get; init; } = moduleNames;
    public string SolutionPath { get; set; } = solutionPath;

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="SolutionPath"></param>
    /// <param name="ModuleName"></param>
    public async Task CreateModuleAsync(string moduleName)
    {
        string moduleDir = Path.Combine(SolutionPath, "src", "Modules");

        if (!Directory.Exists(moduleDir))
        {
            Directory.CreateDirectory(moduleDir);
        }
        if (Directory.Exists(Path.Combine(moduleDir, moduleName)))
        {
            Console.WriteLine("⚠️ 该模块已存在");
            return;
        }

        // 基础类
        string projectPath = Path.Combine(moduleDir, moduleName);
        await Console.Out.WriteLineAsync($"🚀 create module:{moduleName} ➡️ {projectPath}");

        // global usings
        string usingsContent = GetGlobalUsings(moduleName);
        usingsContent = usingsContent.Replace("${Module}", moduleName);
        await AssemblyHelper.GenerateFileAsync(projectPath, "GlobalUsings.cs", usingsContent, true);

        // project file
        string targetVersion = Const.Version;
        string csprojFiles = Directory.GetFiles(Path.Combine(SolutionPath, Config.ApiPath), $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (csprojFiles != null)
        {
            targetVersion = AssemblyHelper.GetTargetFramework(csprojFiles) ?? Const.Version;
        }
        string csprojContent = GetCsProjectContent(targetVersion);
        await AssemblyHelper.GenerateFileAsync(projectPath, $"{moduleName}{Const.CSharpProjectExtention}", csprojContent);

        // create dirs
        Directory.CreateDirectory(Path.Combine(projectPath, "Models"));
        Directory.CreateDirectory(Path.Combine(projectPath, "Manager"));
        Directory.CreateDirectory(Path.Combine(projectPath, "Controllers"));
        // 模块文件
        await AssemblyHelper.GenerateFileAsync(projectPath, "InitModule.cs", GetInitModuleContent(moduleName));
        await AssemblyHelper.GenerateFileAsync(projectPath, "ServiceCollectionExtensions.cs", GetServiceCollectionContent(moduleName));

        try
        {
            await AddDefaultModuleAsync(moduleName);
            await AddModuleConstFieldAsync(moduleName);
            // update solution file
            UpdateSolutionFile(Path.Combine(projectPath, $"{moduleName}{Const.CSharpProjectExtention}"));

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace + ex.InnerException);
        }
    }

    /// <summary>
    /// 添加模块常量
    /// </summary>
    public async Task AddModuleConstFieldAsync(string moduleName)
    {
        string moduleConstPath = Path.Combine(SolutionPath, "src", "Definition", "Entity", "Modules.cs");
        if (File.Exists(moduleConstPath))
        {
            string assemblyPath = Config.IsLight ? Path.Combine(SolutionPath, "src", "Definition")
                : Path.Combine(SolutionPath, "src", "Definition", "Entity");

            CompilationHelper analyzer = new(assemblyPath);
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

    public void CleanModule(string moduleName)
    {

        string entityPath = Path.Combine(SolutionPath, Config.EntityPath, moduleName);
        string entityFrameworkPath = Path.Combine(SolutionPath, Config.EntityFrameworkPath);

        if (Directory.Exists(entityPath))
        {
            List<string> entityFiles = Directory.GetFiles(entityPath, $"*.cs", SearchOption.AllDirectories).ToList();

            // get entity name
            List<string> entityNames = entityFiles.Select(file =>
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                return filename;
            }).ToList();

            Directory.Delete(entityPath, true);

            // 从解决方案移除项目
            ProcessHelper.RunCommand("dotnet", $"sln {SolutionPath} remove {Path.Combine(SolutionPath, "src", "Modules", moduleName + "Mod", $"{moduleName}Mod{Const.CSharpProjectExtention}")}", out string error);
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
        List<string> files = [.. Directory.GetFiles(modulesPath, $"*{Const.CSharpProjectExtention}", SearchOption.AllDirectories)];
        return files.Count != 0 ? files : default;
    }

    private static string GetGlobalUsings(string moduleName)
    {
        string definition = "";
        if (Config.IsLight)
        {
            definition = "Definition.";
        }

        return $$"""
            global using System.ComponentModel.DataAnnotations;
            global using System.Diagnostics;
            global using System.Linq.Expressions;
            global using Application.Const;
            global using Application;
            global using Application.Implement;
            global using ${Module}.Manager;
            global using Ater.Web.Abstraction;
            global using Ater.Web.Core.Models;
            global using Ater.Web.Core.Utils;
            global using Ater.Web.Extension;
            global using {{definition}}Entity;
            global using {{definition}}Entity.{{moduleName}};
            global using {{definition}}EntityFramework;
            global using {{definition}}EntityFramework.DBProvider;
            global using Microsoft.AspNetCore.Authorization;
            global using Microsoft.Extensions.DependencyInjection;
            global using Microsoft.AspNetCore.Mvc;
            global using Microsoft.EntityFrameworkCore;
            global using Microsoft.Extensions.Logging;
            
            """;
    }

    /// <summary>
    /// 默认csproj内容
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    private static string GetCsProjectContent(string version = Const.Version)
    {
        return $"""
            <Project Sdk="Microsoft.NET.Sdk">
            	<PropertyGroup>
            		<TargetFramework>{version}</TargetFramework>
            		<ImplicitUsings>enable</ImplicitUsings>
                    <GenerateDocumentationFile>true</GenerateDocumentationFile>
            		<Nullable>enable</Nullable>
                    <NoWarn>1701;1702;1591</NoWarn>
            	</PropertyGroup>
            	<ItemGroup>
            	    <ProjectReference Include="..\..\Application\Application.csproj" />
                    <ProjectReference Include="..\..\Infrastructure\Ater.Web.Extension\Ater.Web.Extension.csproj" />
            	</ItemGroup>
            </Project>
            """;
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

    private static string GetServiceCollectionContent(string moduleName)
    {
        return $$"""
            namespace {{moduleName}};
            /// <summary>
            /// 服务注入扩展
            /// </summary>
            public static class ServiceCollectionExtensions
            {
                /// <summary>
                /// 添加模块服务
                /// </summary>
                /// <param name="services"></param>
                /// <returns></returns>
                public static IServiceCollection Add{{moduleName}}Services(this IServiceCollection services)
                {
                    services.Add{{moduleName}}Managers();
                    // TODO:注入其他服务
                    return services;
                }

                /// <summary>
                /// 注入Manager服务
                /// </summary>
                /// <param name="services"></param>
                public static IServiceCollection Add{{moduleName}}Managers(this IServiceCollection services)
                {
                    // TODO: 注入Manager服务
                    return services;
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
                Console.WriteLine("add project ➡️ solution failed:" + error);
            }
            else
            {
                Console.WriteLine("✅ add project ➡️ solution!");
            }
        }
        string csprojFiles = Directory.GetFiles(Path.Combine(SolutionPath, Config.ApiPath), $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (File.Exists(csprojFiles))
        {
            // 添加到主服务
            if (!ProcessHelper.RunCommand("dotnet", $"add {csprojFiles} reference {projectPath}", out string error))
            {
                Console.WriteLine("add project reference failed:" + error);
            }
        }
    }

    /// <summary>
    /// 添加默认模块
    /// </summary>
    private async Task AddDefaultModuleAsync(string moduleName)
    {
        if (!ModuleNames.Contains(moduleName))
        {
            return;
        }
        string studioPath = AssemblyHelper.GetStudioPath();
        string sourcePath = Path.Combine(studioPath, "Modules", moduleName);
        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"🦘 no default {moduleName}, just init it!");
            return;
        }

        string databasePath = Path.Combine(SolutionPath, Config.EntityFrameworkPath);
        string entityPath = Path.Combine(SolutionPath, Config.EntityPath, $"{moduleName}");
        string modulePath = Path.Combine(SolutionPath, "src", "Modules", moduleName);

        Console.WriteLine("🚀 copy module files");
        // copy entities
        CopyModuleFiles(Path.Combine(sourcePath, "Entity"), entityPath);

        // copy module files
        CopyModuleFiles(sourcePath, modulePath);

        Console.WriteLine("🚀 update ContextBase DbSet");
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
            var plural = PluralizationProvider.Pluralize(entityName);
            string propertyString = $@"public DbSet<{entityName}> {plural} {{ get; set; }}";

            if (!compilation.PropertyExist(plural))
            {
                compilation.AddClassProperty(propertyString);
                Console.WriteLine($"  ℹ️ add new property {plural} ➡️ ContextBase");
            }
        });

        dbContextContent = compilation.SyntaxRoot!.ToFullString();
        File.WriteAllText(dbContextFile, dbContextContent);
        // update globalusings.cs
        string globalUsingsFile = Path.Combine(databasePath, "GlobalUsings.cs");
        string globalUsingsContent = File.ReadAllText(globalUsingsFile);

        string newLine = @$"global using Entity.{moduleName};";
        if (!globalUsingsContent.Contains(newLine))
        {
            Console.WriteLine($"  ℹ️ add new using {newLine} ➡️ GlobalUsings");
            globalUsingsContent = globalUsingsContent.Replace("global using Entity;", $"global using Entity;{Environment.NewLine}{newLine}");
            File.WriteAllText(globalUsingsFile, globalUsingsContent);
        }

        // 重新生成依赖注入服务
        string applicationPath = Path.Combine(SolutionPath, Config.ApplicationPath);
        string entityFrameworkPath = Path.Combine(SolutionPath, Config.EntityFrameworkPath);
        var applicationName = Config.ApplicationPath.Split(Path.DirectorySeparatorChar).Last();
        var entityFrameworkName = Config.EntityFrameworkPath.Split(Path.DirectorySeparatorChar).Last();

        string content = ManagerGenerate.GetManagerDIExtensions(applicationPath, applicationName);
        await IOHelper.WriteToFileAsync(Path.Combine(applicationPath, "ManagerServiceCollectionExtensions.cs"), content);

        // 初始化内容
        if (moduleName.Equals("SystemMod"))
        {
            string apiPath = Path.Combine(SolutionPath, Config.ApiPath);
            //InitSystemModule(apiPath);
        }
    }

    /// <summary>
    /// 复制模块文件
    /// </summary>
    /// <param name="sourceDir"></param>`
    /// <param name="destinationDir"></param>
    /// <param name="recursive"></param>
    private static void CopyModuleFiles(string sourceDir, string destinationDir)
    {
        // 获取源目录信息
        DirectoryInfo dir = new(sourceDir);
        // 检查源目录是否存在
        if (!dir.Exists) { return; }

        // 缓存目录，以便开始复制
        DirectoryInfo[] dirs = dir.GetDirectories();

        // 创建目标目录
        Directory.CreateDirectory(destinationDir);

        // 获取源目录中的文件并复制到目标目录
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);

            Console.WriteLine($"  ℹ️ copy {file.Name} ➡️ {targetFilePath}");
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

    /// <summary>
    /// 系统模块初始化内容
    /// </summary>
    /// <param name="apiPath"></param>
    private static void InitSystemModule(string apiPath)
    {
        string initFilePath = Path.Combine(apiPath, "Worker", "InitDataTask.cs");
        if (!File.Exists(initFilePath))
        {
            return;
        }

        string content = File.ReadAllText(initFilePath);

        string replaceStr = "// [InitModule]";
        if (content.Contains(replaceStr))
        {
            string initContent = "await SystemMod.InitModule.InitializeAsync(provider);";
            content = content.Replace(replaceStr, initContent);
        }

        File.WriteAllText(initFilePath, content);
    }
}
