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
        var configPath = GetConfigPath();

        var file = new FileInfo(Path.Combine(configPath, Config.ConfigFileName));
        var path = file == null
            ? Path.Combine(configPath, Config.ConfigFileName)
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
        var configPath = GetConfigPath();
        var file = new FileInfo(Path.Combine(configPath, Config.ConfigFileName));
        if (file == null)
        {
            Console.WriteLine($"config file not found , please run droplet confing init");
            return default;
        }
        var path = file.FullName;
        var config = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<ConfigOptions>(config);
        return options ?? new ConfigOptions();
    }

    public static void EditConfigFile()
    {
        var configPath = GetConfigPath();
        var file = new FileInfo(Path.Combine(configPath, Config.ConfigFileName));
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

    /// <summary>
    /// 获取config目录路径，优先为sln解决方案目录，如果没有保存到LocalApplicationData
    /// </summary>
    /// <returns></returns>
    private static string GetConfigPath()
    {
        var currentDir = new DirectoryInfo(Environment.CurrentDirectory);
        var solutionPath = AssemblyHelper.GetSlnFile(currentDir, "*.sln", currentDir.Root);
        string configPath;
        if (solutionPath == null)
        {
            Console.WriteLine("can't find sln file, use system user dir");
            configPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else
        {
            configPath = solutionPath.Directory.FullName;
        }
        return configPath;
    }
}