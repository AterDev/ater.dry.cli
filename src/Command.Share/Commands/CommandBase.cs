namespace Command.Share.Commands;

public class CommandBase
{
    protected List<string> Instructions = [];

    public static async Task GenerateFileAsync(string dir, string fileName, string content, bool cover = false)
    {
        if (!Directory.Exists(dir))
        {
            _ = Directory.CreateDirectory(dir);
        }
        string filePath = Path.Combine(dir, fileName);
        if (!File.Exists(filePath) || cover)
        {
            await File.WriteAllTextAsync(filePath, content, new UTF8Encoding(false));
            Console.WriteLine(@$"  ℹ️ generate file {fileName}.");
        }
        else
        {
            Console.WriteLine($"  🦘 Skip exist file: {fileName}.");
        }
    }
}
