using System.CommandLine;
using Entity;

namespace CommandLine;

public class CommandBuilder
{
    public RootCommand RootCommand { get; set; }
    public CommandBuilder()
    {
        RootCommand = new RootCommand("Welcome to use dry cli, a tool to help you generate source code!")
        {
            Name = "dry"
        };
    }

    public RootCommand Build()
    {
        AddDto();
        AddManager();
        AddApi();
        AddNgService();
        AddRequest();
        AddStudio();
        return RootCommand;
    }

    public void AddStudio()
    {
        Command studioCommand = new("studio", "studio management");
        Command update = new("update", "update studio");

        update.SetHandler(CommandRunner.UpdateStudio);
        studioCommand.SetHandler(CommandRunner.RunStudioAsync);

        studioCommand.AddCommand(update);
        RootCommand.Add(studioCommand);
    }

    /// <summary>
    /// DTO命令
    /// </summary>
    public void AddDto()
    {
        // dto 生成命令
        Command dtoCommand = new("dto", "generate entity dto files");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> outputOption = new(["--output", "-o"],
            "output project directory");
        outputOption.SetDefaultValue(PathConst.SharePath);
        Option<bool> forceOption = new(["--force", "-f"],
            "force overwrite file");
        forceOption.SetDefaultValue(true);

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
        Command reqCommand = new("request", "generate request service and interface using openApi json");
        reqCommand.AddAlias("request");
        Argument<string> url = new("OpenApi Url", "openApi json file url");
        Option<string> outputOption = new(["--output", "-o"])
        {
            IsRequired = true,
            Description = "output path"
        };

        Option<RequestLibType> typeOption = new(["--type", "-t"], "request lib type:axios or NgHttp");
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
        Command apiCommand = new("manager", "generate dtos, datastore, manager");
        apiCommand.AddAlias("manager");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> dtoOption = new(["--dto", "-d"],
            "dto project directory");
        dtoOption.SetDefaultValue(Config.SharePath);
        Option<string> storeOption = new(["--manager", "-m"],
            "application project directory");
        storeOption.SetDefaultValue(Config.ApplicationPath);
        Option<string> typeOption = new(["--type", "-t"],
            "api type, valid values:rest/grpc/graph");
        Option<bool> forceOption = new(["--force", "-f"],
           "force overwrite file");
        forceOption.SetDefaultValue(false);

        typeOption.SetDefaultValue("rest");
        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(storeOption);
        apiCommand.AddOption(forceOption);

        apiCommand.SetHandler(
            CommandRunner.GenerateManagerAsync, path, dtoOption, storeOption, forceOption);

        RootCommand.Add(apiCommand);
    }
    /// <summary>
    /// 后端接口及仓储、dto
    /// </summary>
    public void AddApi()
    {
        // api 生成命令
        Command apiCommand = new("webapi", "generate dtos, datastore, manager,api controllers");
        apiCommand.AddAlias("api");
        Argument<string> path = new("entity path", "The entity file path");
        Option<string> dtoOption = new(["--dto", "-d"],
            "dto project director");
        dtoOption.SetDefaultValue(Config.SharePath);

        Option<string> managerOption = new(["--manager", "-m"],
            "manager and datastore project directory");
        managerOption.SetDefaultValue(Config.ApplicationPath);

        Option<string> apiOption = new(["--output", "-o"],
            "api controller project directory");
        apiOption.SetDefaultValue(Config.ApiPath);

        Option<string> suffixOption = new(["--suffix", "-s"],
            "the controller suffix");
        suffixOption.SetDefaultValue("Controller");
        Option<string> typeOption = new(["--type", "-t"],
            "api type, valid values:rest/grpc/graph, just support rest");
        Option<bool> forceOption = new(["--force", "-f"],
      "force overwrite file");
        forceOption.SetDefaultValue(false);

        typeOption.SetDefaultValue("rest");
        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(managerOption);
        apiCommand.AddOption(apiOption);
        apiCommand.AddOption(suffixOption);
        apiCommand.AddOption(forceOption);

        apiCommand.SetHandler(
            CommandRunner.GenerateApiAsync, path, dtoOption, managerOption, apiOption, suffixOption, forceOption);

        RootCommand.Add(apiCommand);
    }

    public void AddNgService()
    {
        Command ngCommand = new("angular", "generate angular service and interface using openApi json");
        ngCommand.AddAlias("ng");
        Argument<string> url = new("OpenApi Json", "openApi json file url or local path");
        Option<string> outputOption = new(["--output", "-o"])
        {
            IsRequired = true,
            Description = "angular project root path"
        };
        ngCommand.AddArgument(url);
        ngCommand.AddOption(outputOption);
        ngCommand.SetHandler(CommandRunner.GenerateNgAsync, url, outputOption);

        RootCommand.Add(ngCommand);
    }
}
