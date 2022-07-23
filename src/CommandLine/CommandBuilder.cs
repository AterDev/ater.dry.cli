using System.CommandLine;

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
        ConfigOptions = ConfigCommand.ReadConfigFile();
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
        return RootCommand;
    }

    /// <summary>
    /// config command 
    /// </summary>
    /// <returns></returns>
    public void AddConfig()
    {
        var configCommand = new Command("config", "config management");
        var edit = new Command("edit", "edit config");
        var init = new Command("init", "init config");
        edit.SetHandler(() => ConfigCommand.EditConfigFile());
        init.SetHandler(() => ConfigCommand.InitConfigFileAsync());
        configCommand.SetHandler(() => { });
        configCommand.AddCommand(edit);
        configCommand.AddCommand(init);
        RootCommand.Add(configCommand);
    }
    /// <summary>
    /// DTO命令
    /// </summary>
    public void AddDto()
    {
        var executor = new CommandRunner();
        // dto 生成命令
        var dtoCommand = new Command("dto", "generate entity dto files");
        var path = new Argument<string>("entity path", "The entity file path");
        var outputOption = new Option<string>(new[] { "--output", "-o" },
            "output project directory");
        outputOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        var forceOption = new Option<bool>(new[] { "--force", "-f" },
            "force overwrite file");
        forceOption.SetDefaultValue(false);

        dtoCommand.AddArgument(path);
        dtoCommand.AddOption(outputOption);
        dtoCommand.AddOption(forceOption);
        dtoCommand.SetHandler(async (string entity, string output, bool force) =>
        {
            await CommandRunner.GenerateDtoAsync(entity, output, force);
        }, path, outputOption, forceOption);

        RootCommand.Add(dtoCommand);
    }
    /// <summary>
    /// 前端请求命令
    /// </summary>
    public void AddRequest()
    {
        var executor = new CommandRunner();
        var reqCommand = new Command("request", "generate request service and interface using openApi json");
        reqCommand.AddAlias("request");
        var url = new Argument<string>("OpenApi Url", "openApi json file url");
        var outputOption=new Option<string>(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "output path"
        };

        var typeOption = new Option<RequestLibType>(new []{"--type","-t"},"request lib type:axios or angularHttpClient");
        reqCommand.AddArgument(url);
        reqCommand.AddOption(outputOption);
        reqCommand.AddOption(typeOption);
        reqCommand.SetHandler(async (string url, string output, RequestLibType libType) =>
        {
            await CommandRunner.GenerateRequestAsync(url, output, libType);
        }, url, outputOption, typeOption);

        RootCommand.Add(reqCommand);
    }

    /// <summary>
    /// 后端manager服务
    /// </summary>
    public void AddManager()
    {
        var executor = new CommandRunner();
        // api 生成命令
        var apiCommand = new Command("manager", "generate dtos, datastore, manager");
        apiCommand.AddAlias("manager");
        var path = new Argument<string>("entity path", "The entity file path");
        var dtoOption = new Option<string>(new[] { "--dto", "-d" },
            "dto project directory");
        dtoOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        var storeOption = new Option<string>(new[] { "--manager", "-m" },
            "application project directory");
        storeOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.StorePath));
        var typeOption = new Option<string>(new[] { "--type", "-t" },
            "api type, valid values:rest/grpc/graph");
        typeOption.SetDefaultValue("rest");
        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(storeOption);

        apiCommand.SetHandler(
            async (string entity, string dto, string store) =>
            {
                await CommandRunner.GenerateManagerAsync(entity, dto, store);
            }, path, dtoOption, storeOption);

        RootCommand.Add(apiCommand);
    }
    /// <summary>
    /// 后端接口及仓储、dto
    /// </summary>
    public void AddApi()
    {
        var executor = new CommandRunner();
        // api 生成命令
        var apiCommand = new Command("webapi", "generate dtos, datastore, manager,api controllers");
        apiCommand.AddAlias("api");
        var path = new Argument<string>("entity path", "The entity file path");
        var dtoOption = new Option<string>(new[] { "--dto", "-d" },
            "dto project director");
        dtoOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        var storeOption = new Option<string>(new[] { "--datastore", "-s" },
            "dataStore project directory");
        storeOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.StorePath));
        var apiOption = new Option<string>(new[] { "--output", "-o" },
            "api controller project directory");
        apiOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.ApiPath));
        var contextOption = new Option<string>(new[] { "--contextName", "-c" },
            "the entityframework dbcontext name, default ContextBase");
        contextOption.SetDefaultValue("ContextBase");
        var typeOption = new Option<string>(new[] { "--type", "-t" },
            "api type, valid values:rest/grpc/graph");
        typeOption.SetDefaultValue("rest");
        apiCommand.AddArgument(path);
        apiCommand.AddOption(dtoOption);
        apiCommand.AddOption(storeOption);
        apiCommand.AddOption(apiOption);
        apiCommand.AddOption(contextOption);

        apiCommand.SetHandler(
            async (string entity, string dto, string store, string output, string context) =>
        {
            await CommandRunner.GenerateApi(entity, dto, store, output);
        }, path, dtoOption, storeOption, apiOption, contextOption);

        RootCommand.Add(apiCommand);
    }
    /// <summary>
    /// 添加文档
    /// </summary>
    public void AddDoc()
    {
        var executor = new CommandRunner();
        var docCommand = new Command("doc", "generate typescript interface using openApi json");
        docCommand.AddAlias("doc");
        var url = new Argument<string>("OpenApi Url", "openApi json file url");
        var  outputOption=new Option<string>(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "generate markdown doc"
        };
        docCommand.AddArgument(url);
        docCommand.AddOption(outputOption);
        docCommand.SetHandler(async (string url, string output) =>
        {
            await CommandRunner.GenerateDocAsync(url, output);
        }, url, outputOption);

        RootCommand.Add(docCommand);
    }
    public void AddNgService()
    {
        var executor = new CommandRunner();
        var ngCommand = new Command("angular", "generate angular service and interface using openApi json");
        ngCommand.AddAlias("ng");
        var url = new Argument<string>("OpenApi Url", "openApi json file url");
        var  outputOption=new Option<string>(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "angular project root path"
        };
        ngCommand.AddArgument(url);
        ngCommand.AddOption(outputOption);
        ngCommand.SetHandler(async (string url, string output) =>
        {
            await CommandRunner.GenerateNgAsync(url, output);
        }, url, outputOption);

        RootCommand.Add(ngCommand);
    }
    public void AddView()
    {
        var executor = new CommandRunner();
        // view生成命令
        var viewCommand = new Command("view", "generate front view page, only support angular. ");
        viewCommand.AddAlias("view");
        var entityArgument = new Argument<string>("entity path","The entity file path, like path/xxx.cs");
        var dtoOption = new Option<string>(new[] { "--dto", "-d" },"dto project directory");
        dtoOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        var outputOption= new Option<string>(new[] { "--output", "-o" },"angular project root path")
        {
            IsRequired = true,
        };

        viewCommand.AddArgument(entityArgument);
        viewCommand.AddOption(dtoOption);
        viewCommand.AddOption(outputOption);
        viewCommand.SetHandler(async (string entity, string dtoPath, string output) =>
        {
            await CommandRunner.GenerateNgPagesAsync(entity, dtoPath, output);
        }, entityArgument, dtoOption, outputOption);
        RootCommand.Add(viewCommand);
    }
}
