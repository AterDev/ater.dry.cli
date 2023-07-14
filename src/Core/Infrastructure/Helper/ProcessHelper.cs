using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Core.Infrastructure.Helper;
/// <summary>
/// 调用帮助类
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// 运行命令
    /// </summary>
    /// <param name="command">命令程序</param>
    /// <param name="args">参数</param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static bool RunCommand(string command, string? args, out string output)
    {
        var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        output = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (string.IsNullOrWhiteSpace(output))
        {
            output = process.StandardOutput.ReadToEnd();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 执行一组命令
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    public static string ExecuteCommands(string[] commands)
    {
        string shell, argument;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            shell = "cmd.exe";
            argument = "/c";
        }
        else
        {
            shell = "/bin/bash";
            argument = "-c";
        }

        string commandString = string.Join(" && ", commands);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"{argument} \"{commandString}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }
}
