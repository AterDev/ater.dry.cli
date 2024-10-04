using System.Diagnostics;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using CodeGenerator.Helper;
using Entity;
using Microsoft.Extensions.Logging;
using Share.EntityFramework.DBProvider;
using Share.Infrastructure.Helper;

namespace Command.Share.Commands;
public class StudioCommand(ILogger<StudioCommand> logger)
{
    private readonly ILogger<StudioCommand> _logger = logger;

    public async Task RunStudioAsync()
    {
        _logger.LogInformation("🙌 Welcome Ater studio!");
        string studioPath = AssemblyHelper.GetStudioPath();

        int sleepTime = 1500;
        // 检查并更新
        string version = AssemblyHelper.GetCurrentToolVersion();
        if (File.Exists(Path.Combine(studioPath, $"{version}.txt")))
        {
            _logger.LogInformation("😊 Already latest version!");
        }
        else
        {
            UpdateStudio();
        }

        _logger.LogInformation("🚀 start studio...");
        // 运行
        string shell = "dotnet";
        var port = GetAvailablePort();
        _logger.LogInformation("可用端口:" + port);

        string url = $"http://localhost:{port}";
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"./{Const.StudioFileName} --urls \"{url}\"",
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
        await Task.Delay(sleepTime).ConfigureAwait(false);

        // 启动浏览器
        try
        {
            Process pr = Process.Start(url);
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
                _logger.LogInformation("start browser failed:" + ex.Message);
            }
        }
        process.WaitForExit();
    }

    /// <summary>
    /// 升级studio
    /// </summary>
    public void UpdateStudio()
    {
        _logger.LogInformation($"☑️ check&update studio...");

        string[] copyFiles = new string[]
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

        string version = AssemblyHelper.GetCurrentToolVersion();
        string toolRootPath = AssemblyHelper.GetToolPath();
        string zipPath = Path.Combine(toolRootPath, Const.StudioZip);
        string templatePath = Path.Combine(toolRootPath, Const.TemplateZip);

        if (!File.Exists(zipPath))
        {
            _logger.LogInformation($"not found studio.zip in:{toolRootPath}");
            return;
        }
        string studioPath = AssemblyHelper.GetStudioPath();
        string dbFile = Path.Combine(studioPath, ContextBase.DbName);
        string tempDbFile = Path.Combine(Path.GetTempPath(), ContextBase.DbName);

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
            string sourceFile = Path.Combine(toolRootPath, file + ".dll");
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, Path.Combine(studioPath, file + ".dll"), true);
            }
        });

        // copy runtimes目录
        string runtimesDir = Path.Combine(toolRootPath, "runtimes");
        if (Directory.Exists(runtimesDir))
        {
            string targetDir = Path.Combine(studioPath, "runtimes");
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
        _logger.LogInformation("✅ update complete!");
    }


    /// <summary>
    /// 获取可用端口
    /// </summary>
    /// <returns></returns>
    /// <summary>
    /// 获取可用端口
    /// </summary>
    /// <returns></returns>
    public static int GetAvailablePort(int alternative = 9160)
    {
        var defaultPort = 19160;
        var properties = IPGlobalProperties.GetIPGlobalProperties();

        var endPointsTcp = properties.GetActiveTcpListeners();
        foreach (var endPoint in endPointsTcp)
        {
            if (endPoint.Port == defaultPort) return alternative;
        }

        var endPointsUdp = properties.GetActiveUdpListeners();
        foreach (var endPoint in endPointsUdp)
        {
            if (endPoint.Port == defaultPort) return alternative;
        }
        return defaultPort;
    }
    /// <summary>
    /// 下载或更新模板
    /// </summary>
    public void UpdateTemplate()
    {
        // 安装模板
        if (!ProcessHelper.RunCommand("dotnet", "new list atapi", out string _))
        {
            if (!ProcessHelper.RunCommand("dotnet", "new install ater.web.templates", out _))
            {
                _logger.LogInformation("⚠️ ater.web.templates install failed!");
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
