using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Definition.EntityFramework.DBProvider;
using Definition.Infrastructure;

namespace Command.Share.Commands;
public class StudioCommand
{
    public static void RunStudio()
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
                Console.WriteLine("start browser failed:" + ex.Message);
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
            "Microsoft.CodeAnalysis.Workspaces",
            "Microsoft.CodeAnalysis",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Relational",
            "Microsoft.CodeAnalysis.CSharp.Workspaces",
            "Humanizer",
            "Microsoft.IdentityModel.Tokens",
            "Microsoft.EntityFrameworkCore.Sqlite",
            "Microsoft.OpenApi",
            "CodeGenerator",
            "SharpYaml",
            "Microsoft.Data.Sqlite",
            "Mapster",
            "Microsoft.OpenApi.Readers",
            "Definition",
            "Microsoft.IdentityModel.JsonWebTokens",
            "Command.Share",
            "Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost",
            "System.IdentityModel.Tokens.Jwt",
            "Microsoft.Extensions.DependencyModel",
            "Microsoft.CodeAnalysis.Workspaces.MSBuild",
            "NuGet.Versioning",
            "System.Composition.TypedParts",
            "PluralizeService.Core",
            "System.Composition.Hosting",
            "System.Composition.Convention",
            "SQLitePCLRaw.core",
            "Ater.Web.Abstraction",
            "Microsoft.Build.Locator",
            "Microsoft.IdentityModel.Logging",
            "Ater.Web.Core",
            "SQLitePCLRaw.provider.e_sqlite3",
            "Mapster.Core",
            "System.Composition.Runtime",
            "System.Composition.AttributedModel",
            "Microsoft.IdentityModel.Abstractions",
            "Microsoft.Bcl.AsyncInterfaces",
            "SQLitePCLRaw.batteries_v2"
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
        var dbFile = Path.Combine(studioPath, ContextBase.DbName);
        var tempDbFile = Path.Combine(Path.GetTempPath(), ContextBase.DbName);

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

        // copy runtimes目录
        var runtimesDir = Path.Combine(toolRootPath, "runtimes");
        if (Directory.Exists(runtimesDir))
        {
            var targetDir = Path.Combine(studioPath, "runtimes");
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }
            try
            {
                Directory.CreateSymbolicLink(targetDir, runtimesDir);
            }
            catch (Exception)
            {
                IOHelper.CopyDirectory(runtimesDir, targetDir);
            }
        }

        // create version file
        File.Create(Path.Combine(studioPath, $"{version}.txt")).Close();

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
}
