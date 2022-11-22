﻿using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace Command.Share.Commands;
public class StudioCommand
{
    public static void RunStudio()
    {
        Console.WriteLine("welcome ater studio!");
        string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        string targetFramework = "net7.0";
        string packageId = "ater.droplet.cli";

        string zipPath = Path.Combine(userPath, ".dotnet/tools/.store", packageId, version, packageId, version, "tools", targetFramework, "any", "studio.zip");


        var studioPath = Path.Combine(appPath, "AterStudio");
        var file = new FileInfo(Path.Combine(studioPath, Config.StudioFileName));

        if (!file.Exists)
        {
            // 初始化，解压内容
            Console.WriteLine($"init studio...");
            if (!File.Exists(zipPath))
            {
                Console.WriteLine("not found studio.zip");
                return;
            }
            ZipFile.ExtractToDirectory(zipPath, studioPath, true);
            file = new(Path.Combine(studioPath, Config.StudioFileName));
        }

        Console.WriteLine("start studio...");
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
                Arguments = $"./{Config.StudioFileName}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                WorkingDirectory = studioPath
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
        process.Dispose();
    }

    public static void Update()
    {
        Console.WriteLine($"update studio...");
        string appPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        string targetFramework = "net7.0";
        string packageId = "ater.droplet.cli";

        string zipPath = Path.Combine(userPath, ".dotnet/tools/.store", packageId, version, packageId, version, "tools", targetFramework, "any", "studio.zip");

        if (!File.Exists(zipPath))
        {
            Console.WriteLine("not found studio.zip");
            return;
        }
        ZipFile.ExtractToDirectory(zipPath, Path.Combine(appPath, "AterStudio"), true);
        Console.WriteLine("update complete!");
    }
}
