using System.CommandLine;

namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        await ConfigCommand.InitConfigFileAsync();

        var root = new CommandBuilder().Build();
        return await root.InvokeAsync(args);
    }

    private static async Task OldAsync()
    {
        var gtCommand = new RootCommand("Welcome use droplet cli, a tool to help you generate source code!")
        {
            Name = "droplet"
        };

        // 集成命令
        var genCommand = new Command("generate", "代码生成");
        genCommand.AddAlias("g");
        genCommand.AddOption(new Option<string>(new[] { "--entity", "-e" })
        {
            IsRequired = true,
            Description = "实体模型文件路径"
        });
        genCommand.AddOption(new Option<string>(new[] { "--service", "-s" })
        {
            Description = "数据仓储服务的项目路径,默认为./Core.Services",
        });
        genCommand.AddOption(new Option<string>(new[] { "--share", "-share" })
        {
            Description = "Share项目路径，默认为./Share"
        });
        genCommand.AddOption(new Option<string>(new[] { "--web", "-w" })
        {
            IsRequired = true,
            Description = "网站项目路径"
        });
        genCommand.AddOption(new Option<string>(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "前端项目根目录"
        });

        gtCommand.Add(genCommand);
        //return await gtCommand.InvokeAsync("");
    }
}
