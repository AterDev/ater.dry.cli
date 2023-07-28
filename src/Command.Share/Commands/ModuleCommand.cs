using Core.Infrastructure;
using PluralizeService.Core;

namespace Command.Share.Commands;
/// <summary>
/// 模块命令
/// </summary>
public class ModuleCommand
{
    public const string ModuleUserLogs = "UserLogs";
    public const string ModuleCMS = "CMS";
    public const string ModuleSystemLogs = "SystemLogs";
    public const string ModuleConfiguration = "Configuration";
    public static List<string> ModuleNames { get; } = new()
    {
        ModuleCMS,
        ModuleSystemLogs,
        ModuleUserLogs,
        ModuleConfiguration
    };

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <param name="moduleName"></param>
    public static async Task CreateModuleAsync(string solutionPath, string moduleName)
    {
        var moduleDir = Path.Combine(solutionPath, "src", "Modules");

        if (!Directory.Exists(moduleDir))
        {
            Directory.CreateDirectory(moduleDir);
        }
        if (Directory.Exists(Path.Combine(moduleDir, moduleName)))
        {
            throw new Exception("该模块已存在");
        }

        // 基础类
        string projectPath = Path.Combine(moduleDir, moduleName);
        await Console.Out.WriteLineAsync($"🚀 create module:{moduleName} ➡️ {projectPath}");
        string tplContent = GenerateBase.GetTplContent("Implement.RestControllerBase.tpl");
        tplContent = tplContent.Replace(TplConst.NAMESPACE, moduleName);
        string infrastructruePath = Path.Combine(projectPath, "Infrastructure");
        await AssemblyHelper.GenerateFileAsync(infrastructruePath, "RestControllerBase.cs", tplContent);

        // global usings
        string usingsContent = GetGlobalUsings();
        usingsContent = usingsContent.Replace("${Module}", moduleName);
        await AssemblyHelper.GenerateFileAsync(projectPath, "GlobalUsings.cs", usingsContent);

        // get target version 
        string targetVersion = "7.0";
        var csprojFiles = Directory.GetFiles(Path.Combine(solutionPath, Config.ApiPath), $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (csprojFiles != null)
        {
            targetVersion = AssemblyHelper.GetTargetFramework(csprojFiles) ?? "7.0";
        }
        string csprojContent = GetCsProjectContent(targetVersion);
        await AssemblyHelper.GenerateFileAsync(projectPath, $"{moduleName}{Const.CSharpProjectExtention}", csprojContent);

        try
        {
            await AddDefaultModuleAsync(solutionPath, moduleName);
            // update solution file
            UpdateSolutionFile(solutionPath, Path.Combine(projectPath, $"{moduleName}{Const.CSharpProjectExtention}"));

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace + ex.InnerException);
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
        List<string> files = Directory.GetFiles(modulesPath, $"*{Const.CSharpProjectExtention}", SearchOption.AllDirectories).ToList();
        return files.Any() ? files : default;
    }

    private static string GetGlobalUsings()
    {
        return """
            global using System.Diagnostics;
            global using System.Linq.Expressions;
            global using Application.Const;
            global using Application.IManager;
            global using Application.Implement;
            global using ${Module}.Infrastructure;
            global using Entity.{Module}Entities;
            global using Ater.Web.Core.Models;
            global using Ater.Web.Core.Utils;
            global using Microsoft.AspNetCore.Authorization;
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
    private static string GetCsProjectContent(string version = "7.0")
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
            		<FrameworkReference Include="Microsoft.AspNetCore.App" />
            	</ItemGroup>
            	<ItemGroup>
            	    <ProjectReference Include="..\..\Application\Application.csproj" />
            	</ItemGroup>
            </Project>
            """;
    }

    /// <summary>
    /// 使用dotnet sln add
    /// </summary>
    /// <param name="dirPath"></param>
    /// <param name="projectPath"></param>
    private static void UpdateSolutionFile(string dirPath, string projectPath)
    {
        var slnFile = AssemblyHelper.GetSlnFile(new DirectoryInfo(dirPath), "*.sln");
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
        var csprojFiles = Directory.GetFiles(Path.Combine(dirPath, Config.ApiPath), $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
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
    /// <param name="moduleName"></param>
    /// <param name="solutionPath"></param>
    private static async Task AddDefaultModuleAsync(string solutionPath, string moduleName)
    {
        if (!ModuleNames.Contains(moduleName))
        {
            return;
        }
        var studioPath = AssemblyHelper.GetStudioPath();
        var sourcePath = Path.Combine(studioPath, "Modules", moduleName);
        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"🦘 no default {moduleName}, just init it!");
            return;
        }

        var databasePath = Path.Combine(solutionPath, "src", "Database", "EntityFramework");
        var entityPath = Path.Combine(solutionPath, Config.EntityPath, $"{moduleName}Entities");
        var modulePath = Path.Combine(solutionPath, "src", "Modules", moduleName);

        Console.WriteLine("🚀 copy module files");
        // copy entities
        CopyModuleFiles(Path.Combine(sourcePath, "Entities"), entityPath);
        // copy module files
        CopyModuleFiles(sourcePath, modulePath);
        // copy datastore
        var storeFiles = Directory.GetFiles(Path.Combine(sourcePath, "Application"), "*.cs")
            .ToList();
        var queryStorePath = Path.Combine(solutionPath, Config.ApplicationPath, "QueryStore");
        var commandStorePath = Path.Combine(solutionPath, Config.ApplicationPath, "CommandStore");
        storeFiles.ForEach(file =>
        {
            var filename = Path.GetFileName(file);
            if (filename.EndsWith("QueryStore.cs"))
            {
                File.Copy(file, Path.Combine(queryStorePath, filename));
            }
            else if (file.EndsWith("CommandStore.cs"))
            {
                File.Copy(file, Path.Combine(commandStorePath, filename));
            }
        });

        Console.WriteLine("🚀 update ContextBase DbSet");
        var dbContextFile = Path.Combine(databasePath, "ContextBase.cs");
        var dbContextContent = File.ReadAllText(dbContextFile);

        var compilation = new CompilationHelper(databasePath);
        compilation.AddSyntaxTree(dbContextContent);

        var entityFiles = new DirectoryInfo(Path.Combine(sourcePath, "Entities")).GetFiles("*.cs").ToList();

        entityFiles.ForEach(file =>
        {
            var entityName = Path.GetFileNameWithoutExtension(file.Name);
            var plural = PluralizationProvider.Pluralize(entityName);
            var propertyString = $@"public DbSet<{entityName}> {plural} {{ get; set; }}";
            if (!compilation.PropertyExist(plural))
            {
                Console.WriteLine($"  ℹ️ add new property {plural} ➡️ ContextBase");
                compilation.AddClassProperty(propertyString);
            }
        });
        dbContextContent = compilation.SyntaxRoot!.ToFullString();
        File.WriteAllText(dbContextFile, dbContextContent);

        // 重新生成DataStore和依赖注入服务
        var applicationPath = Path.Combine(solutionPath, Config.ApplicationPath);
        var entityFrameworkPath = Path.Combine(solutionPath, Config.EntityFrameworkPath);
        var applicationName = Config.ApplicationPath.Split(Path.DirectorySeparatorChar).Last();
        var entityFrameworkName = Config.EntityFrameworkPath.Split(Path.DirectorySeparatorChar).Last();
        var content = ManagerGenerate.GetDataStoreContext(entityFrameworkPath, entityFrameworkName);
        await IOHelper.WriteToFileAsync(Path.Combine(entityFrameworkPath, "DataStoreContext.cs"), content);

        content = ManagerGenerate.GetManagerDIExtensions(solutionPath, applicationName);
        await IOHelper.WriteToFileAsync(Path.Combine(applicationPath, "ManagerServiceCollectionExtensions.cs"), content);

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
        var dir = new DirectoryInfo(sourceDir);
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
            if (subDir.Name == "Entities")
            {
                continue;
            }
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyModuleFiles(subDir.FullName, newDestinationDir);
        }
    }

}
