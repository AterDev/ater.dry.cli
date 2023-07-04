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
        // 基础类
        var modulePath = Path.Combine(solutionPath, "src", "Modules", name);
        await Console.Out.WriteLineAsync($"🆕 create module:{name} to {modulePath}");
        var tplContent = GenerateBase.GetTplContent("Implement.RestControllerBase.tpl");
        tplContent = tplContent.Replace(TplConst.NAMESPACE, name);
        await AssemblyHelper.GenerateFileAsync(modulePath, "RestControllerBase.cs", tplContent);

        // global usings
        var usingsContent = GetGlobalUsings();
        usingsContent = usingsContent.Replace("${Module}", name);
        await AssemblyHelper.GenerateFileAsync(modulePath, "GlobalUsings.cs", usingsContent);
    }

    /// <summary>
    /// 获取模块列表
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <returns>file path</returns>
    public static List<string>? GetModulesPaths(string solutionPath)
    {
        var modulesPath = Path.Combine(solutionPath, "src", "Modules");
        if (!Directory.Exists(modulesPath))
        {
            return default;
        }
        var files = Directory.GetFiles(modulesPath, "*.csproj", SearchOption.AllDirectories).ToList();
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
            global using ${Module}.IManager;
            global using ${Module}.Infrastructure;
            global using Core.Utils;
            global using Microsoft.AspNetCore.Authorization;
            global using Microsoft.AspNetCore.Mvc;
            global using Microsoft.EntityFrameworkCore;
            global using Microsoft.Extensions.Logging;
            global using Share.Models;
            """;
    }
}
