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

        // angular 生成命令
        var ngCommand = new Command("angular", "angular请求服务生成，包括类型接口！");
        ngCommand.AddAlias("ng");
        ngCommand.AddOption(new Option<string>(new[] { "--url", "-u" })
        {
            Description = "swagger url,默认为 http://localhost:5002/swagger/app/swagger.json"
        });
        ngCommand.AddOption(new Option<string>(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "前端目录路径"
        });

        // view生成命令
        var viewCommand = new Command("view", "view代码生成，只支持ng项目生成");
        viewCommand.AddAlias("view");
        viewCommand.AddOption(new Option<string>(new[] { "--name", "-n" })
        {
            IsRequired = true,
            Description = "实体类名称"
        });
        viewCommand.AddOption(new Option<string>(new[] { "--share", "-s" })
        {
            Description = "share项目路径,默认为./Share"
        });
        viewCommand.AddOption(new Option<string>(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "前端目录路径"
        });


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

        gtCommand.Add(ngCommand);
        gtCommand.Add(viewCommand);
        gtCommand.Add(genCommand);
        //return await gtCommand.InvokeAsync("");
    }
}
