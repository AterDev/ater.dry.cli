
namespace Droplet.CommandLine.Commands;

public class CommandBase
{
    protected List<string> Instructions = new();

    public virtual async Task GenerateFileAsync(string dir, string fileName, string content, bool cover = false)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var filePath = Path.Combine(dir, fileName);
        if (!File.Exists(filePath) || cover)
            await File.WriteAllTextAsync(filePath, content);
    }
}
