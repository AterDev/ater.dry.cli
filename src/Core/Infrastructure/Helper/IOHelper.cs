namespace Core.Infrastructure.Helper;
/// <summary>
/// 文件IO帮助类
/// </summary>
public class IOHelper
{
    /// <summary>
    /// move dir
    /// </summary>
    /// <param name="source">dir path</param>
    /// <param name="target">dir path</param>
    /// <param name="needBackup"></param>
    public static void MoveDirectory(string source, string target, bool needBackup = false)
    {
        if (!Directory.Exists(source))
        {
            return;
        }

        if (needBackup)
        {
            string backPath = $"{target}.bak";
            if (Directory.Exists(backPath))
            {
                Directory.Delete(backPath, true);
            }
            if (Directory.Exists(target))
            {
                Directory.Move(target, backPath);
                Directory.Delete(target, true);
            }
        }
        else
        {
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
        }
        Directory.Move(source, target);
    }

    public static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    /// <summary>
    /// 获取代码文件
    /// </summary>
    public static string[] GetCodeFiles(string dirPath)
    {
        return Directory.GetFiles(
             dirPath,
             $"*.cs",
             SearchOption.AllDirectories)
            .Where(f => !f.Replace(dirPath, "").StartsWith("/obj")
                && !f.Replace(dirPath, "").StartsWith("/bin")
                && !f.EndsWith(".Assembly.cs"))
            .ToArray();
    }

    /// <summary>
    /// use UTF8 without bom as default
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task WriteToFileAsync(string filePath, string content)
    {
        await File.WriteAllTextAsync(filePath, content, new UTF8Encoding(false));
    }
}
