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
    protected string GetTplContent(string tplPath)
    {
        tplPath = "CodeGenerator.Templates." + tplPath;
        // 读取模板文件
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(tplPath);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path">目录</param>
    /// <param name="fileName">文件名称</param>
    /// <param name="content">文件内容</param>
    protected void SaveToFile(string path, string fileName, string content)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, fileName), content);
        Console.WriteLine($"Created file {Path.Combine(path, fileName)}.");
    }
}
