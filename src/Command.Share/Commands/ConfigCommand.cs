using System.Diagnostics;
using System.Text.Json;

namespace Command.Share.Commands;

public class ConfigCommand
{
    /// <summary>
    /// 初始化配置文件
    /// </summary>
    public static async Task InitConfigFileAsync()
    {
        string configPath = GetConfigPath();

        FileInfo file = new(Path.Combine(configPath, Config.ConfigFileName));
        string path = file == null
            ? Path.Combine(configPath, Config.ConfigFileName)
            : file.FullName;

        if (File.Exists(path))
        {
            return;
        }
        ConfigOptions options = new();
        string content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, content, Encoding.UTF8);
        Console.WriteLine("Init config file success");
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    public static ConfigOptions? ReadConfigFile()
    {
        string configPath = GetConfigPath();
        FileInfo file = new(Path.Combine(configPath, Config.ConfigFileName));
        if (file == null)
        {
            Console.WriteLine($"config file not found , please run droplet confing init");
            return default;
        }
        string path = file.FullName;
        string config = File.ReadAllText(path);
        ConfigOptions? options = JsonSerializer.Deserialize<ConfigOptions>(config);
        return options ?? new ConfigOptions();
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