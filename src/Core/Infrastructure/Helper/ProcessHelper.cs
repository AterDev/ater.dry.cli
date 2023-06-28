using System.Diagnostics;

namespace Core.Infrastructure.Helper;
/// <summary>
/// 调用帮助类
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// 运行命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
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
}
