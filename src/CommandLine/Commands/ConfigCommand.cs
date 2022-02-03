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
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, Config.ConfigPath);

        if (File.Exists(path))
        {
            Console.WriteLine("Load config file.");
            return;
        }
        var options = new ConfigOptions();
        var content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, content, Encoding.UTF8);
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    public static ConfigOptions ReadConfigFile()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, Config.ConfigPath);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("file not found ", path);
        }
        var config = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<ConfigOptions>(config);
        return options ?? new ConfigOptions();
    }

    public static void EditConfigFile()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, Config.ConfigPath);
        if (!File.Exists(path))
        {
            Console.WriteLine("config file not exist!");
            return;
        }
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
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
    }
}
