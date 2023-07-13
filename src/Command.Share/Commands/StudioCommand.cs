using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;

using Core.Entities;
using Core.Infrastructure;

using LiteDB;

using NuGet.Versioning;

namespace Command.Share.Commands;
public class StudioCommand
{
    public static async Task RunStudioAsync()
    {
        Console.WriteLine("🙌 welcome ater studio!");
        string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var studioPath = Path.Combine(appPath, "AterStudio");

        int sleepTime = 1500;
        // 检查并更新
        string version = AssemblyHelper.GetCurrentToolVersion();
        if (File.Exists(Path.Combine(studioPath, $"{version}.txt")))
        {
            Console.WriteLine("😊 Already latest version!");
        }
        else
        {
            // 更新程序
            UpdateStudio();
        }
        // 更新项目信息
        await UpdateProjectAsync();

        Console.WriteLine("🚀 start studio...");
        // 运行
        string shell = "dotnet";
        var url = "http://localhost:9160";
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"./{Config.StudioFileName} --urls \"{url}\"",
                UseShellExecute = false,
                CreateNoWindow = false,
                //RedirectStandardOutput = true,
                WorkingDirectory = studioPath,
                //RedirectStandardError = true,
                //StandardErrorEncoding = Encoding.UTF8,
                //StandardOutputEncoding = Encoding.UTF8,
            },
        };
        process.Start();
        Thread.Sleep(sleepTime);
        // 启动浏览器
        try
        {
            var pr = Process.Start(url);
            pr.Close();
        }
        catch (Exception ex)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
                {
                    CreateNoWindow = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Console.WriteLine("start browserr failed:" + ex.Message);
            }
        }
        process.WaitForExit();
    }

    /// <summary>
    /// 升级studio
    /// </summary>
    public static void UpdateStudio()
    {
        Console.WriteLine($"☑️ check&update studio...");

        var copyFiles = new string[]
        {
            "Microsoft.CodeAnalysis.CSharp",
            "Microsoft.CodeAnalysis",
            "LiteDB",
            "SharpYaml",
            "Microsoft.OpenApi",
            "CodeGenerator",
            "Microsoft.OpenApi.Readers",
            "Core",
            "Command.Share",
            "Datastore",
            "NuGet.Versioning",
            "PluralizeService.Core"
        };

        var version = AssemblyHelper.GetCurrentToolVersion();
        var toolRootPath = AssemblyHelper.GetToolPath();
        var zipPath = Path.Combine(toolRootPath, Const.StudioZip);
        var templatePath = Path.Combine(toolRootPath, Const.TemplateZip);

        if (!File.Exists(zipPath))
        {
            Console.WriteLine($"not found studio.zip in:{toolRootPath}");
            return;
        }
        var studioPath = AssemblyHelper.GetStudioPath();
        // 删除旧文件
        if (Directory.Exists(studioPath))
        {
            Directory.Delete(studioPath, true);
        }

        // 解压
        if (File.Exists(templatePath))
        {
            ZipFile.ExtractToDirectory(templatePath, studioPath, true);
        }
        ZipFile.ExtractToDirectory(zipPath, studioPath, true);
        // create version file
        File.Create(Path.Combine(studioPath, $"{version}.txt")).Close();
        // copy其他文件以及runtimes目录
        copyFiles.ToList().ForEach(file =>
        {
            var sourceFile = Path.Combine(toolRootPath, file + ".dll");
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, Path.Combine(studioPath, file + ".dll"), true);
            }
        });
        UpdateTemplate();
        Console.WriteLine("✅ update complete!");
    }

    /// <summary>
    /// 下载或更新模板
    /// </summary>
    public static void UpdateTemplate()
    {
        // 安装模板
        if (!ProcessHelper.RunCommand("dotnet", "new list atapi", out string _))
        {
            if (!ProcessHelper.RunCommand("dotnet", "new install ater.web.templates", out _))
            {
                Console.WriteLine("⚠️ ater.web.templates install failed!");
            }
        }
        else
        {
            if (ProcessHelper.RunCommand("dotnet", "new update", out string _))
            {
            }
        }
    }

    /// <summary>
    /// 更新项目数据
    /// </summary>
    public static async Task UpdateProjectAsync()
    {
        var localDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AterStudio");

        // 更新数据库
        var connectionString = $"Filename={Path.Combine(localDir, "droplet.db")};Upgrade=true;initialSize=5MB";
        using var db = new LiteDatabase(connectionString);
        var collection = db.GetCollection<Project>();
        var projects = collection.FindAll().ToList();
        foreach (var project in projects)
        {
            var solutionDir = project.Path;
            if (File.Exists(project.Path))
            {
                solutionDir = Path.Combine(project.Path, "..");
            }
            solutionDir = Path.Combine(solutionDir, Config.ConfigFileName);

            // read config file
            string configJson = await File.ReadAllTextAsync(solutionDir);
            ConfigOptions? options = System.Text.Json.JsonSerializer.Deserialize<ConfigOptions>(configJson);

            if (options != null)
            {
                Const.PROJECT_ID = options.ProjectId;
                // 添加projectId标识 
                if (options.ProjectId == Guid.Empty)
                {
                    options.ProjectId = Guid.NewGuid();
                    Const.PROJECT_ID = options.ProjectId;
                }
                // 7.0配置更新
                if (NuGetVersion.Parse(options.Version) == NuGetVersion.Parse("7.0.0"))
                {
                    options.DtoPath = "src/" + options.DtoPath;
                    options.EntityPath = "src/" + options.EntityPath;
                    options.DbContextPath = "src/" + options.DbContextPath;
                    options.StorePath = "src/" + options.StorePath;
                    options.ApiPath = "src/" + options.ApiPath;
                }
                if (options.SolutionType == null)
                {
                    var type = AssemblyHelper.GetSolutionType(project.Path);
                    options.SolutionType = type;
                }

                string content = System.Text.Json.JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(solutionDir, content, Encoding.UTF8);
                Console.WriteLine($" Update {project.Name} config file success");

                // 库中版本数据
                project.Version = options.Version;
                project.SolutionType = options.SolutionType;
                collection.Update(project);
            }
            else
            {
                Console.WriteLine("config file parsing error! : " + solutionDir);
            }
        }
        db.Dispose();
    }

    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
