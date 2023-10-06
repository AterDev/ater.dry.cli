using System.Diagnostics;
using System.Net.NetworkInformation;

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
