using System.CommandLine;

namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var root = new CommandBuilder().Build();
        // 读取配置
        ConfigCommand.ReadConfigFile();
        return await root.InvokeAsync(args);
    }

    private static async Task OldAsync()
    {
        var gtCommand = new RootCommand("Welcome use droplet cli, a tool to help you generate source code!")
        {
            Name = "droplet"
        };
        // config 配置命令

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

        // api 生成命令
        var apiCommand = new Command("webapi", "aspnetcore webapi代码生成，模型，仓储！");
        apiCommand.AddAlias("api");
        apiCommand.AddOption(new Option<string>(new[] { "--entity", "-e" })
        {
            IsRequired = true,
            Description = "实体模型文件路径"
        });
        apiCommand.AddOption(new Option<string>(new[] { "--service", "-s" })
        {
            Description = "数据仓储服务的项目路径，默认为./Core.Services",
        });
        apiCommand.AddOption(new Option<string>(new[] { "--web", "-w" })
        {
            Description = "网站项目路径，默认为./App.Api"
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

        // 执行方法
        /*ngCommand.Handler = CommandHandler.Create<string, string>(
             async (url, output) =>
             {
                 if (string.IsNullOrEmpty(url))
                 {
                     url = "http://localhost:5002/swagger/app/swagger.json";
                 }
                 await cmd.GenerateNgAsync(url, output);
             });

        apiCommand.Handler = CommandHandler.Create<string, string, string>(
             (entity, service, web) =>
             {
                 if (string.IsNullOrEmpty(service))
                 {
                     service = Config.SERVICE_PATH;
                 }
                 if (string.IsNullOrEmpty(web))
                 {
                     web = Config.API_PATH;
                 }
                 cmd.GenerateApi(entity, service, web, Config.DTO_PATH);
             });
        viewCommand.Handler = CommandHandler.Create<string, string, string>(
            (name, share, output) =>
            {
                if (string.IsNullOrEmpty(share))
                {
                    share = "./" + Config.SHARE_NAMESPACE;
                }
                cmd.GenerateNgPages(name, share, output);
            });

        genCommand.Handler = CommandHandler.Create<string, string, string, string, string>(
          async (entity, service, share, web, output) =>
          {
              if (string.IsNullOrEmpty(service))
              {
                  service = "./" + Config.SERVICE_NAMESPACE;
              }
              if (string.IsNullOrEmpty(share))
              {
                  service = "./" + Config.SHARE_NAMESPACE;
              }
              await cmd.GenerateAsync(entity, service, share, web, output);
          });*/

        gtCommand.Add(ngCommand);
        gtCommand.Add(apiCommand);
        gtCommand.Add(viewCommand);
        gtCommand.Add(genCommand);
        //return await gtCommand.InvokeAsync("");
    }
}
