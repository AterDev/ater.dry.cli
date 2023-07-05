using Core.Infrastructure;

namespace Command.Share.Commands;
/// <summary>
/// 模块命令
/// </summary>
public class ModuleCommand
{
    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <param name="name"></param>
    public static async Task CreateModuleAsync(string solutionPath, string name)
    {
        var moduleDir = Path.Combine(solutionPath, "src", "Modules");

        if (!Directory.Exists(moduleDir))
        {
            Directory.CreateDirectory(moduleDir);
        }
        // 基础类
        string projectPath = Path.Combine(moduleDir, name);
        await Console.Out.WriteLineAsync($"🆕 create module:{name} to {projectPath}");
        string tplContent = GenerateBase.GetTplContent("Implement.RestControllerBase.tpl");
        tplContent = tplContent.Replace(TplConst.NAMESPACE, name);
        string infrastructruePath = Path.Combine(projectPath, "Infrastructure");
        await AssemblyHelper.GenerateFileAsync(infrastructruePath, "RestControllerBase.cs", tplContent);

        // global usings
        string usingsContent = GetGlobalUsings();
        usingsContent = usingsContent.Replace("${Module}", name);
        await AssemblyHelper.GenerateFileAsync(projectPath, "GlobalUsings.cs", usingsContent);

        // csproject 
        // get target version 
        string? targetVersion = AssemblyHelper.GetTargetFramework(Path.Combine(solutionPath, "src", "Http.API", "Http.API.csproj"));
        string csprojContent = GetCsProjectContent(targetVersion ?? "7.0");
        await AssemblyHelper.GenerateFileAsync(projectPath, $"{name}.csproj", csprojContent);

        // update solution file
        UpdateSolutionFile(solutionPath, Path.Combine(projectPath, $"{name}.csproj"));
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
        List<string> files = Directory.GetFiles(modulesPath, "*.csproj", SearchOption.AllDirectories).ToList();
        return files.Any() ? files : default;
    }

    private static string GetGlobalUsings()
    {
        return """
            global using System.Diagnostics;
            global using System.Linq.Expressions;
            global using Application.Const;
            global using Application.Implement;
            global using Application.Interface;
            global using ${Module}.Infrastructure;
            global using Core.Utils;
            global using Microsoft.AspNetCore.Authorization;
            global using Microsoft.AspNetCore.Mvc;
            global using Microsoft.EntityFrameworkCore;
            global using Microsoft.Extensions.Logging;
            global using Share.Models;
            """;
    }

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
            if (!ProcessHelper.RunCommand("dotnet", $"sln {slnFile.FullName} add {projectPath}", out string error))
            {
                Console.WriteLine("add project to solution failed:" + error);
            }
            else
            {
                Console.WriteLine("✅ add project to solution!");
            }

        }
    }
}
