
namespace GeneratorForNode;

public class GenerateBase
{
    /// <summary>
    /// 获取模板内容
    /// </summary>
    /// <param name="tplPath"></param>
    public static string GetTplContent(string tplPath)
    {
        tplPath = "GeneratorForNode.Templates." + tplPath;
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
}
