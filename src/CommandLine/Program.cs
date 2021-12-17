using System.CommandLine;
using System.CommandLine.Invocation;

namespace Droplet.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var cmdTet = new RootCommands();
        var test = new Test();
        test.TestEFCoreEntityTypeModel();
        //await cmdTet.GenerateNgAsync("http://localhost:5002/swagger/v1/swagger.json", "./webapp");
        //var projectPath = @"E:\niltor\Incubate\src";
        //var entityPath = System.IO.Path.Combine(projectPath, "Core.Entity", "Account.cs");
        //var servicePath = Path.Combine(projectPath, "Core.Services");
        //var webPath = Path.Combine(projectPath, "App.Api");
        //cmdTet.GenerateApi(entityPath, servicePath, webPath);

        //cmdTet.GenerateNgAsync("http://localhost:5000/swagger/user/swagger.json", "../clients/test").Wait();
        //cmdTet.GenerateAsync(@"../../../../../src/Core/Entities/MemberCard.cs", @"../../../../../src/services", @"../../../../../src/Web", "../../../../../clients/admin-front").Wait();
        //return 0;

        // 读取配置
        Config.ReadConfigFile();

        var gtCommand = new RootCommand("geethin commands")
        {
            Name = "gt"
        };
        // config 配置命令
        var configCommand = new Command("config", "配置文件管理");
        configCommand.AddCommand(new Command("edit", "编辑config")
        {
            Description = "编辑config",
            Handler = CommandHandler.Create(() =>
            {
                Config.EditConfigFile();
            })
        });
        configCommand.AddCommand(new Command("init", "初始化config")
        {
            Description = "初始化config",
            Handler = CommandHandler.Create(async () =>
            {
                await Config.InitConfigFileAsync();
            })
        });

        // dto 生成命令
        var dtoCommand = new Command("dto", "dto模型生成或更新");
        dtoCommand.AddAlias("dto");
        dtoCommand.AddOption(new Option<string>(new[] { "--entity", "-e" })
        {
            IsRequired = true,
            Description = "实体模型文件路径"
        });
        dtoCommand.AddOption(new Option<string>(new[] { "--output", "-o" })
        {
            Description = "dto 输出目录，默认为./Share/Models"
        });


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
        var cmd = new RootCommands();
        dtoCommand.Handler = CommandHandler.Create<string, string>(
            (entity, output) =>
            {
                if (string.IsNullOrEmpty(output))
                {
                    output = Config.DTO_PATH;
                }
                cmd.GenerateDto(entity, output);
            });

        ngCommand.Handler = CommandHandler.Create<string, string>(
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
          });

        configCommand.Handler = CommandHandler.Create<string, string>(
            (init, edit) =>
            {

            });
        gtCommand.Add(dtoCommand);
        gtCommand.Add(ngCommand);
        gtCommand.Add(apiCommand);
        gtCommand.Add(viewCommand);
        gtCommand.Add(genCommand);
        gtCommand.Add(configCommand);
        return await gtCommand.InvokeAsync(args);
    }
}
