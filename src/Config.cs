using System.Diagnostics;
using System.Text.Json;

namespace Droplet.CommandLine;

public static class Config
{
    static readonly string ConfigPath = "./.gtcli-config.json";

    public static string ENTITY_NAMESPACE = "Entity";
    public static string SERVICE_NAMESPACE = "Services";
    public static string WEB_NAMESPACE = "App.Api";
    public static string SHARE_NAMESPACE = "Share";
    public static string DBCONTEXT_NAMESPACE = "EntityFrameworkCore";

    public static string CLIENT_PATH = "../clients/webapp";
    public static string API_PATH = "./App.Api";
    public static string SERVICE_PATH = "./Services";
    public static string SHARE_PATH = "./Share";
    public static string DBCONTEXT_PATH = "./EntityFrameworkCore";
    public static string DTO_PATH = "./Share/Models";

    /// <summary>
    /// 初始化配置文件
    /// </summary>
    public static async Task InitConfigFileAsync()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, ConfigPath);

        if (File.Exists(path))
        {
            Console.WriteLine("config file already exist!");
            return;
        }
        var options = new ConfigOptions
        {
            BasePath = "./",

            ApiNamespace = WEB_NAMESPACE,
            EntityNamespace = ENTITY_NAMESPACE,
            ServiceNamespace = SERVICE_NAMESPACE,
            ShareNamespace = SHARE_NAMESPACE,
            DbContextNamespace = DBCONTEXT_NAMESPACE,

            ApiPath = "app/" + WEB_NAMESPACE,
            EntityPath = "src/" + ENTITY_NAMESPACE,
            ServicePath = "src/" + SERVICE_NAMESPACE,
            DbContextPath = "src/EntityFrameworkCore",
            SharePath = "src/" + SHARE_NAMESPACE,

            ClientPath = CLIENT_PATH,
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
        path = Path.Combine(path, ConfigPath);

        if (!File.Exists(path))
        {
            return default;
        }
        var config = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<ConfigOptions>(config);

        WEB_NAMESPACE = options.ApiNamespace;
        ENTITY_NAMESPACE = options.EntityNamespace;
        SERVICE_NAMESPACE = options.ServiceNamespace;
        SHARE_NAMESPACE = options.ShareNamespace;
        DBCONTEXT_NAMESPACE = options.DbContextNamespace;

        CLIENT_PATH = Path.Combine(options.BasePath, options.ClientPath);
        API_PATH = Path.Combine(options.BasePath, options.ApiPath);
        SERVICE_PATH = Path.Combine(options.BasePath, options.ServicePath);
        SHARE_PATH = Path.Combine(options.BasePath, options.SharePath);
        DTO_PATH = Path.Combine(options.BasePath, options.DtoPath);
        DBCONTEXT_PATH = Path.Combine(options.BasePath, options.DbContextPath);

        return options;
    }

    public static void EditConfigFile()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, ConfigPath);
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
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
    }
}
