using System.IO;

namespace CodeGenerator.Test.Hepler;

public static class PathHelper
{

    public static string GetProjectPath()
    {
        string currentDir = Environment.CurrentDirectory;
        if (currentDir.Contains("bin"))
        {
            currentDir = Path.Combine(currentDir, "../../..");
        }
        return currentDir;
    }

    public static string GetProjectFilePath(string path)
    {
        string rootPath = GetProjectPath();
        return Path.Combine(rootPath, path);
    }
}
