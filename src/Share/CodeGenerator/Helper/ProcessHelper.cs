using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace CodeGenerator.Helper;
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

        var outputBuilder = new StringBuilder();
        var outputErrorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {

            if (!string.IsNullOrEmpty(e.Data))
            {
                outputErrorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var errorOutput = outputErrorBuilder.ToString();
        output = outputBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            Console.WriteLine(errorOutput);
            return false;
        }
        else
        {
            Console.WriteLine(output);
            return true;
        }
    }

    /// <summary>
    /// 执行命令，使用cmd/bash
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    public static string ExecuteCommands(params string[] commands)
    {
        string shell, argument;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            shell = "powershell";
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
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var outputBuilder = new StringBuilder();
        var outputErrorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {

            if (!string.IsNullOrEmpty(e.Data))
            {
                outputErrorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var errorOutput = outputErrorBuilder.ToString();
        var output = outputBuilder.ToString();
        return string.IsNullOrWhiteSpace(errorOutput) ? output : errorOutput;
    }


    /// <summary>
    /// 获取可用端口
    /// </summary>
    /// <returns></returns>
    public static int GetAvailablePort(int alternative = 9160)
    {
        var defaultPort = 19160;
        var properties = IPGlobalProperties.GetIPGlobalProperties();

        var connections = properties.GetActiveTcpConnections();
        foreach (var connection in connections)
        {
            if (connection.LocalEndPoint.Port == defaultPort) return alternative;
        }

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

}
