using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace Command.Share.Commands;
public class StudioCommand
{
    public static void RunStudio()
    {
        string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        string targetFramework = "net7.0";
        string packageId = "ater.droplet.cli";

        string zipPath = Path.Combine(userPath, ".dotnet/tools/.store", packageId, version, packageId, version, "tools", targetFramework, "any", "studio.zip");

        FileInfo? file = new(Path.Combine(appPath, "AterStudio", Config.StudioFileName));

        if (file == null)
        {
            // 初始化，解压内容
            Console.WriteLine($"init studio...");
            ZipFile.ExtractToDirectory(zipPath, Path.Combine(appPath, "AterStudio"));
        }

        // 运行
        string path = file.FullName;
        string shell = "dotnet";
        //switch (Environment.OSVersion.Platform)
        //{
        //    case PlatformID.Unix:
        //        shell = "bash";
        //        break;
        //    default:
        //        break;
        //}
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"{path}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                //RedirectStandardError = true,
                //StandardErrorEncoding = Encoding.UTF8,
                //StandardOutputEncoding = Encoding.UTF8,
            }
        };
        _ = process.Start();
        while (!process.StandardOutput.EndOfStream)
        {
            string? line = process.StandardOutput.ReadLine();
            Console.WriteLine(line);
        }
        process.WaitForExit();
        process.Close();
    }
}
