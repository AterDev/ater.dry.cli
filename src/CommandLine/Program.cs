using System.CommandLine;
using System.Text;
using Core.Infrastructure;

namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        await ConfigCommand.InitConfigFileAsync();
        RootCommand root = new CommandBuilder().Build();
        return await root.InvokeAsync(args);
    }
}
