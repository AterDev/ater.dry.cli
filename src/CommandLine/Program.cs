using System.CommandLine;
using Datastore;
using Microsoft.EntityFrameworkCore;

namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        // 数据库更新
        var context = new ContextBase();
        await context.Database.MigrateAsync();

        await ConfigCommand.InitConfigFileAsync();
        RootCommand root = new CommandBuilder().Build();
        return await root.InvokeAsync(args);
    }
}
