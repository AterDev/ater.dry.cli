using System.CommandLine;
namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (!args.Contains("studio"))
        {
            await ConfigCommand.InitConfigFileAsync();
        }
        RootCommand root = new CommandBuilder().Build();
        return await root.InvokeAsync(args);
    }
}
