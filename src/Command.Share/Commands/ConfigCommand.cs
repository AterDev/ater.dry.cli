using System.Diagnostics;
using System.Text.Json;

using Core.Entities;
using Core.Infrastructure;

namespace Command.Share.Commands;

public class ConfigCommand
{
    /// <summary>
    /// 初始化配置文件
    /// </summary>
    public static async Task InitConfigFileAsync(string? configPath = null, SolutionType? solutionType = null)
    {
        configPath ??= GetConfigPath();
        Console.WriteLine("use config path:" + configPath);
        FileInfo file = new(Path.Combine(configPath, Config.ConfigFileName));
        string path = file.FullName;

        if (!File.Exists(path))
        {
            ConfigOptions options = new()
            {
                ProjectId = Guid.NewGuid(),
            };
            if (solutionType != null)
            {
                options.SolutionType = solutionType.Value;
            }

            Const.PROJECT_ID = options.ProjectId;
            string content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, content, new UTF8Encoding(false));
            Console.WriteLine("Init config file success:" + path);
        }
    }

    public static async Task UpdateConfigAsync(string path)
    {
        if (!File.Exists(path))
        {
            await Console.Out.WriteLineAsync("file not found:" + path);
            return;
        }
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="configPath"></param>
    public static ConfigOptions? ReadConfigFile(string? configPath = null)
    {
        configPath ??= GetConfigPath();
        FileInfo file = new(Path.Combine(configPath, Config.ConfigFileName));
        if (!file.Exists)
        {
            Console.WriteLine($"config file not found , please run droplet confing init");
            return default;
        }
        string path = file.FullName;
        string config = File.ReadAllText(path);
        ConfigOptions? options = JsonSerializer.Deserialize<ConfigOptions>(config);
        return options;
    }

    public static void EditConfigFile()
    {
        string configPath = GetConfigPath();
        FileInfo file = new(Path.Combine(configPath, Config.ConfigFileName));
        if (file == null)
        {
            Console.WriteLine($"config file not found , please run droplet confing init");
            return;
        }
        string path = file.FullName;
        Process process = new()
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
        _ = process.Start();
        _ = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
    }

    /// <summary>
    /// 获取config目录路径，优先为sln解决方案目录，如果没有保存到LocalApplicationData
    /// </summary>
    /// <returns></returns>
    public static string GetConfigPath()
    {
        DirectoryInfo currentDir = new(Environment.CurrentDirectory);
        FileInfo? solutionPath = AssemblyHelper.GetSlnFile(currentDir, "*.sln", currentDir.Root);
        string configPath;
        if (solutionPath == null)
        {
            configPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else
        {
            configPath = solutionPath.Directory!.FullName;
        }
        return configPath;
    }
}