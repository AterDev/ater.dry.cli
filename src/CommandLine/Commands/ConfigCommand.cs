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
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, Config.ConfigPath);

        if (File.Exists(path))
        {
            Console.WriteLine("config file already exist!");
            return;
        }
        var options = new ConfigOptions
        {
            BasePath = "./",

            ApiNamespace = Config.HTTPAPI_NAMESPACE,
            EntityNamespace = Config.ENTITY_NAMESPACE,
            ServiceNamespace = Config.SERVICE_NAMESPACE,
            ShareNamespace = Config.SHARE_NAMESPACE,
            DbContextNamespace = Config.DBCONTEXT_NAMESPACE,

            ApiPath = "app/" + Config.HTTPAPI_NAMESPACE,
            EntityPath = "src/" + Config.ENTITY_NAMESPACE,
            ServicePath = "src/" + Config.SERVICE_NAMESPACE,
            DbContextPath = "src/EntityFrameworkCore",
            SharePath = "src/" + Config.SHARE_NAMESPACE,

            ClientPath = Config.CLIENT_PATH,
            DtoPath = "src/Share/Models"
        };

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
            return default;
        }
        var config = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<ConfigOptions>(config);

        Config.HTTPAPI_NAMESPACE = options.ApiNamespace;
        Config.ENTITY_NAMESPACE = options.EntityNamespace;
        Config.SERVICE_NAMESPACE = options.ServiceNamespace;
        Config.SHARE_NAMESPACE = options.ShareNamespace;
        Config.DBCONTEXT_NAMESPACE = options.DbContextNamespace;

        Config.CLIENT_PATH = Path.Combine(options.BasePath, options.ClientPath);
        Config.HTTPAPI_PATH = Path.Combine(options.BasePath, options.ApiPath);
        Config.SERVICE_PATH = Path.Combine(options.BasePath, options.ServicePath);
        Config.SHARE_PATH = Path.Combine(options.BasePath, options.SharePath);
        Config.SHAREMODEL_PATH = Path.Combine(options.BasePath, options.DtoPath);
        Config.DBCONTEXT_PATH = Path.Combine(options.BasePath, options.DbContextPath);

        return options;
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
