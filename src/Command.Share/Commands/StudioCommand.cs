using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

using Definition.Infrastructure;

namespace Command.Share.Commands;
public class StudioCommand
{
    public static async Task RunStudioAsync()
    {
        Console.WriteLine("🙌 Welcome Ater studio!");
        var studioPath = AssemblyHelper.GetStudioPath();

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
        var port = ProcessHelper.GetAvailablePort();
        Console.WriteLine("可用端口:" + port);

        var url = $"http://localhost:{port}";
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
            "Microsoft.CodeAnalysis",
            "Microsoft.CodeAnalysis.CSharp",
            "Microsoft.CodeAnalysis.Workspaces",
            "Microsoft.CodeAnalysis.Workspaces.MSBuild",
            "Microsoft.CodeAnalysis.CSharp.Workspaces",
            "Microsoft.Build",
            "Microsoft.Build.Framework",
            "Humanizer",
            "LiteDB",
            "SharpYaml",
            "Microsoft.OpenApi",
            "Microsoft.OpenApi.Readers",
            "Core",
            "CodeGenerator",
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
        var dbFile = Path.Combine(studioPath, "dry.db");
        var tempDbFile = Path.Combine(Path.GetTempPath(), "dry.db");

        // 删除旧文件
        if (Directory.Exists(studioPath))
        {
            if (File.Exists(dbFile))
            {
                File.Copy(dbFile, tempDbFile, true);
            }
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

        if (File.Exists(tempDbFile))
        {
            File.Copy(tempDbFile, dbFile, true);
        }

        // copy其他文件
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

        var db = new DryContext();

        var projects = db.Projects.ToList();
        foreach (var project in projects)
        {
            var solutionDir = project.Path;

            if (File.Exists(project.Path))
            {
                solutionDir = Path.Combine(project.Path, "..");
            }
            if (!Directory.Exists(solutionDir))
            {
                db.Remove(project);
                continue;
            }
            solutionDir = Path.Combine(solutionDir, Config.ConfigFileName);

            // read config file
            if (!File.Exists(solutionDir))
            {
                continue;
            }
            string configJson = await File.ReadAllTextAsync(solutionDir);
            ConfigOptions? options = ConfigOptions.ParseJson(configJson);
            if (options != null)
            {
            }
            else
            {
                Console.WriteLine("config file parsing error! : " + solutionDir);
            }
        }
        db.Dispose();
    }
}
