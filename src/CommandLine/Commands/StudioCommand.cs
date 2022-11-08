using System.Diagnostics;

namespace Droplet.CommandLine.Commands;
internal class StudioCommand
{
    public static void RunStudio()
    {
        var appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var file = new FileInfo(Path.Combine(appPath, "Studio", Config.StudioFileName));
        if (file == null)
        {
            // TODO:下载studio
            Console.WriteLine($"download studio...");
        }

        // 运行
        var path = file.FullName;
        var shell = "dotnet";
        //switch (Environment.OSVersion.Platform)
        //{
        //    case PlatformID.Unix:
        //        shell = "bash";
        //        break;
        //    default:
        //        break;
        //}
        var process = new Process()
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
        process.Start();
        while (!process.StandardOutput.EndOfStream)
        {
            var line = process.StandardOutput.ReadLine();
            Console.WriteLine(line);
        }
        process.WaitForExit();
    }
}
