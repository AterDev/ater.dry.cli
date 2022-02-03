using System.IO;

namespace CodeGenerator.Test.Hepler;

public static class PathHelper
{

    public static string GetProjectPath()
    {
        var currentDir = Environment.CurrentDirectory;
        if (currentDir.Contains("bin"))
        {
            currentDir = Path.Combine(currentDir, "../../..");
        }
        return currentDir;
    }

    public static string GetProjectFilePath(string path)
    {
        var rootPath = GetProjectPath();
        return Path.Combine(rootPath, path);
    }
}
