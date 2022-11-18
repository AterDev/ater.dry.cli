using System.Diagnostics;

namespace Droplet.CommandLine.Commands;
internal class StudioCommand
{
    public static void RunStudio()
    {
        string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        FileInfo? file = new(Path.Combine(appPath, "Studio", Config.StudioFileName));
        if (file == null)
        {
            // TODO:下载studio
            Console.WriteLine($"download studio...");
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
