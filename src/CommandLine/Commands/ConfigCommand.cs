using CodeGenerator.Infrastructure.Helper;
using System.Diagnostics;
using System.Text.Json;

namespace Droplet.CommandLine.Commands;

public class ConfigCommand
{
    /// <summary>
    /// 初始化配置文件
    /// </summary>
    public static async Task InitConfigFileAsync()
    {
        var file = AssemblyHelper.FindFile(new DirectoryInfo(Environment.CurrentDirectory), ".droplet-config.json");
        var solutionPath = AssemblyHelper.FindFile(new DirectoryInfo(Environment.CurrentDirectory), "*.sln");
        if (solutionPath == null)
        {
            Console.WriteLine("can't find sln file");
            return;
        }
        var path = file == null
            ? Path.Combine(solutionPath.DirectoryName!, Config.ConfigPath)
            : file.FullName;

        if (File.Exists(path))
        {
            return;
        }
        var options = new ConfigOptions();
        var content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, content, Encoding.UTF8);
        Console.WriteLine("Init config file success");
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    public static ConfigOptions? ReadConfigFile()
    {
        var file = AssemblyHelper.FindFile(new DirectoryInfo(Environment.CurrentDirectory), ".droplet-config.json");
        if (file == null)
        {
            Console.WriteLine($"config file not found , please run droplet confing init");
            return default;
        }
        var path =  file.FullName;
        var config = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<ConfigOptions>(config);
        return options ?? new ConfigOptions();
    }


    public static void EditConfigFile()
    {
        var file = AssemblyHelper.FindFile(new DirectoryInfo(Environment.CurrentDirectory), ".droplet-config.json");
        if (file == null)
        {
            Console.WriteLine($"config file not found , please run droplet confing init");
            return;
        }
        var path =  file.FullName;
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-c {path}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        _ = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
    }
}
