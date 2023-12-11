using System.Text.Encodings.Web;
using System.Text.Json;

namespace Command.Share;
/// <summary>
/// 更新管理
/// </summary>
public class UpdateManager(string solutionFilPath, string currentVersion)
{
    public static string? ErrorMsg { get; private set; }
    public string SolutionFilePath { get; set; } = solutionFilPath;
    public bool Success { get; set; }
    public string TargetVersion { get; set; } = currentVersion;
    public string CurrentVersion { get; set; } = currentVersion;

    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        TypeInfoResolver = null
    };
}
