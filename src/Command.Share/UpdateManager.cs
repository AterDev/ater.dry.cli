using System.Text.Encodings.Web;
using System.Text.Json;

namespace Command.Share;
/// <summary>
/// 更新管理
/// </summary>
public class UpdateManager
{
    public static string? ErrorMsg { get; private set; }
    public string SolutionFilePath { get; set; }
    public bool Success { get; set; } = false;
    public string TargetVersion { get; set; }
    public string CurrentVersion { get; set; }

    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        TypeInfoResolver = null
    };

    public UpdateManager(string solutionFilPath, string currentVersion)
    {
        SolutionFilePath = solutionFilPath;
        CurrentVersion = currentVersion;
        TargetVersion = currentVersion;
    }

}
