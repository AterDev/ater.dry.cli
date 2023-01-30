using System.CommandLine;
using Core.Infrastructure;

namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        await ConfigCommand.InitConfigFileAsync();
        RootCommand root = new CommandBuilder().Build();
        return await root.InvokeAsync(args);
    }
}
