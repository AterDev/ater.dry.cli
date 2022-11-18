namespace CodeGenerator.Generate;

public class GenerateBase
{
    public GenerateBase()
    {
    }
    /// <summary>
    /// 获取模板内容
    /// </summary>
    /// <param name="tplPath"></param>
    protected static string GetTplContent(string tplPath)
    {
        tplPath = "CodeGenerator.Templates." + tplPath;
        // 读取模板文件
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(tplPath);
        if (stream == null)
        {
            Console.WriteLine("  ❌ can't find tpl file:" + tplPath);
            return "";
        }
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path">目录</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="content">文件内容</param>
    protected static void SaveToFile(string path, string fileName, string content)
    {
        if (!Directory.Exists(path))
        {
            _ = Directory.CreateDirectory(path);
        }

        File.WriteAllText(Path.Combine(path, fileName), content);
        Console.WriteLine($"Created file {Path.Combine(path, fileName)}.");
    }
}
