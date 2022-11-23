﻿using System.CommandLine;
using System.Text.Json;
namespace Droplet.CommandLine;

public class CommandBuilder
{
    public RootCommand RootCommand { get; set; }
    public ConfigOptions ConfigOptions { get; set; }

    public CommandBuilder()
    {
        RootCommand = new RootCommand("Welcome use droplet cli, a tool to help you generate source code!")
        {
            Name = "droplet"
        };
        ConfigOptions = ConfigCommand.ReadConfigFile()!;
        Config.IdType = ConfigOptions.IdType;
        Config.CreatedTimeName = ConfigOptions.CreatedTimeName;
    }

    public RootCommand Build()
    {
        AddConfig();
        AddDto();
        AddManager();
        AddApi();
        AddNgService();
        AddRequest();
        AddView();
        AddDoc();
        AutoSyncToNg();
        AddStudio();
        return RootCommand;
    }


    public void AddStudio()
    {
        System.CommandLine.Command studioCommand = new("studio", "studio management");
        var update = new System.CommandLine.Command("update", "update studio");
        update.SetHandler(StudioCommand.Update);
        studioCommand.SetHandler(StudioCommand.RunStudio);

        studioCommand.AddCommand(update);
        RootCommand.Add(studioCommand);
    }

    /// <summary>
    /// config command 
    /// </summary>
    /// <returns></returns>
    public void AddConfig()
    {
        System.CommandLine.Command configCommand = new("config", "config management");

        System.CommandLine.Command edit = new("edit", "edit config");
        System.CommandLine.Command init = new("init", "init config");

        edit.SetHandler(ConfigCommand.EditConfigFile);
        init.SetHandler(ConfigCommand.InitConfigFileAsync);

        configCommand.AddCommand(edit);
        configCommand.AddCommand(init);
        configCommand.SetHandler(() =>
        {
            ConfigOptions? config = ConfigCommand.ReadConfigFile();
            if (config != null)
            {
                Console.WriteLine(JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            }
        });

        RootCommand.Add(configCommand);
    }
    /// <summary>
    /// DTO命令
    /// </summary>
    public void AddDto()
    {
        // dto 生成命令
        System.CommandLine.Command dtoCommand = new("dto", "generate entity dto files");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> outputOption = new(new[] { "--output", "-o" },
            "output project directory");
        outputOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        Option<bool> forceOption = new(new[] { "--force", "-f" },
            "force overwrite file");
        forceOption.SetDefaultValue(false);

        dtoCommand.AddArgument(path);
        dtoCommand.AddOption(outputOption);
        dtoCommand.AddOption(forceOption);
        dtoCommand.SetHandler(CommandRunner.GenerateDtoAsync, path, outputOption, forceOption);

        RootCommand.Add(dtoCommand);
    }
    /// <summary>
    /// 前端请求命令
    /// </summary>
    public void AddRequest()
    {
        System.CommandLine.Command reqCommand = new("request", "generate request service and interface using openApi json");
        reqCommand.AddAlias("request");
        Argument<string> url = new("OpenApi Url", "openApi json file url");
        Option<string> outputOption = new(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "output path"
        };

        Option<RequestLibType> typeOption = new(new[] { "--type", "-t" }, "request lib type:axios or NgHttp");
        reqCommand.AddArgument(url);
        reqCommand.AddOption(outputOption);
        reqCommand.AddOption(typeOption);
        reqCommand.SetHandler(CommandRunner.GenerateRequestAsync, url, outputOption, typeOption);

        RootCommand.Add(reqCommand);
    }

    /// <summary>
    /// 后端manager服务
    /// </summary>
    public void AddManager()
    {
        // api 生成命令
        System.CommandLine.Command apiCommand = new("manager", "generate dtos, datastore, manager");
        apiCommand.AddAlias("manager");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> dtoOption = new(new[] { "--dto", "-d" },
            "dto project directory");
        dtoOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        Option<string> storeOption = new(new[] { "--manager", "-m" },
            "application project directory");
        storeOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.StorePath));
        Option<string> typeOption = new(new[] { "--type", "-t" },
            "api type, valid values:rest/grpc/graph");
        typeOption.SetDefaultValue("rest");
        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(storeOption);

        apiCommand.SetHandler(
            CommandRunner.GenerateManagerAsync, path, dtoOption, storeOption);

        RootCommand.Add(apiCommand);
    }
    /// <summary>
    /// 后端接口及仓储、dto
    /// </summary>
    public void AddApi()
    {
        // api 生成命令
        System.CommandLine.Command apiCommand = new("webapi", "generate dtos, datastore, manager,api controllers");
        apiCommand.AddAlias("api");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> dtoOption = new(new[] { "--dto", "-d" },
            "dto project director");
        dtoOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));

        Option<string> managerOption = new(new[] { "--manager", "-m" },
            "manager and datastore project directory");
        managerOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.StorePath));

        Option<string> apiOption = new(new[] { "--output", "-o" },
            "api controller project directory");
        apiOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.ApiPath));

        Option<string> suffixOption = new(new[] { "--suffix", "-s" },
            "the controller suffix");
        suffixOption.SetDefaultValue("Controller");
        Option<string> typeOption = new(new[] { "--type", "-t" },
            "api type, valid values:rest/grpc/graph, just support rest");
        typeOption.SetDefaultValue("rest");
        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(managerOption);
        apiCommand.AddOption(apiOption);
        apiCommand.AddOption(suffixOption);

        apiCommand.SetHandler(
            CommandRunner.GenerateApiAsync, path, dtoOption, managerOption, apiOption, suffixOption);

        RootCommand.Add(apiCommand);
    }
    /// <summary>
    /// 添加文档
    /// </summary>
    public void AddDoc()
    {
        System.CommandLine.Command docCommand = new("doc", "generate typescript interface using openApi json");
        docCommand.AddAlias("doc");
        Argument<string> url = new("OpenApi Url", "openApi json file url");
        Option<string> outputOption = new(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "generate markdown doc"
        };
        docCommand.AddArgument(url);
        docCommand.AddOption(outputOption);
        docCommand.SetHandler(CommandRunner.GenerateDocAsync, url, outputOption);

        RootCommand.Add(docCommand);
    }
    public void AddNgService()
    {
        System.CommandLine.Command ngCommand = new("angular", "generate angular service and interface using openApi json");
        ngCommand.AddAlias("ng");
        Argument<string> url = new("OpenApi Json", "openApi json file url or local path");
        Option<string> outputOption = new(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "angular project root path"
        };
        ngCommand.AddArgument(url);
        ngCommand.AddOption(outputOption);
        ngCommand.SetHandler(CommandRunner.GenerateNgAsync, url, outputOption);

        RootCommand.Add(ngCommand);
    }
    public void AddView()
    {
        // view生成命令
        System.CommandLine.Command viewCommand = new("view", "generate front view page, only support angular. ");
        viewCommand.AddAlias("view");
        Argument<string> entityArgument = new("entity path", "The entity file path, like path/xxx.cs");
        Option<string> dtoOption = new(new[] { "--dto", "-d" }, "dto project directory");
        dtoOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        Option<string> outputOption = new(new[] { "--output", "-o" }, "angular project root path")
        {
            IsRequired = true,
        };

        viewCommand.AddArgument(entityArgument);
        viewCommand.AddOption(dtoOption);
        viewCommand.AddOption(outputOption);
        viewCommand.SetHandler(CommandRunner.GenerateNgPagesAsync, entityArgument, dtoOption, outputOption);
        RootCommand.Add(viewCommand);
    }

    /// <summary>
    /// 自动同步angular页面
    /// </summary>
    public void AutoSyncToNg()
    {
        System.CommandLine.Command syncCommand = new("sync", "sync angular service & page from swagger.json to ClientApp, please use this in Http.API path");

        syncCommand.SetHandler(
            async () =>
            {
                await CommandRunner.SyncToAngularAsync(
                    "./swagger.json", ConfigOptions.EntityPath, ConfigOptions.DtoPath, ConfigOptions.ApiPath);
            }
            );
        RootCommand.Add(syncCommand);
    }
}
