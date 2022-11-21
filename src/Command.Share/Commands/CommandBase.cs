namespace Command.Share.Commands;

public class CommandBase
{
    protected List<string> Instructions = new();

    public virtual async Task GenerateFileAsync(string dir, string fileName, string content, bool cover = false)
    {
        if (!Directory.Exists(dir))
        {
            _ = Directory.CreateDirectory(dir);
        }
        string filePath = Path.Combine(dir, fileName);
        if (!File.Exists(filePath) || cover)
        {
            await File.WriteAllTextAsync(filePath, content);
            Console.WriteLine(@$"  👍 generate file {fileName}.");
        }
        else
        {
            Console.WriteLine($"  📣 Skip exist file: {fileName}.");
        }
    }
}
