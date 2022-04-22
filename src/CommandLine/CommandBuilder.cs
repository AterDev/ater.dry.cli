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
        AddApi();
        AddTSModel();
        AddNgService();
        AddView();
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
        var init = new Command("init", "intit config");
        edit.SetHandler(() => ConfigCommand.EditConfigFile());
        init.SetHandler(() => ConfigCommand.InitConfigFileAsync());
        configCommand.SetHandler(() => { });
        configCommand.AddCommand(edit);
        configCommand.AddCommand(init);
        RootCommand.Add(configCommand);
    }

    public void AddDto()
    {
        var executor = new CommandRunner();
        // dto 生成命令
        var dtoCommand = new Command("dto", "generate entity dto files");
        var path = new Argument<string>("entity path", "The entity file path");
        var outputOption = new Option<string>(new[] { "--output", "-o" },
            "output project directory，default ./Share");
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

    public void AddApi()
    {
        var executor = new CommandRunner();
        // api 生成命令
        var apiCommand = new Command("webapi", "generate dtos, datastore, api controllers");
        apiCommand.AddAlias("api");
        var path = new Argument<string>("entity path", "The entity file path");
        var dtoOption = new Option<string>(new[] { "--dto", "-d" },
            "dto project directory，default ./Share");
        dtoOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.DtoPath));
        var storeOption = new Option<string>(new[] { "--datastore", "-s" },
            "dataStore project directory，default ./Http.Application");
        storeOption.SetDefaultValue(Path.Combine(ConfigOptions.RootPath, ConfigOptions.StorePath));
        var apiOption = new Option<string>(new[] { "--output", "-o" },
            "api controller project directory，default ./Http.API");
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
    /// 添加typescript interface
    /// </summary>
    public void AddTSModel()
    {
        var executor = new CommandRunner();
        var tsCommand = new Command("ts", "generate typescript interface using openApi json");
        tsCommand.AddAlias("ts");
        var url = new Argument<string>("OpenApi Url", "openApi json file url");
        var  outputOption=new Option<string>(new[] { "--output", "-o" })
        {
            IsRequired = true,
            Description = "ts models output path"
        };
        tsCommand.AddArgument(url);
        tsCommand.AddOption(outputOption);
        tsCommand.SetHandler(async (string url, string output) =>
        {
            await CommandRunner.GenerateTSAsync(url, output);
        }, url, outputOption);

        RootCommand.Add(tsCommand);
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
        var dtoOption = new Option<string>(new[] { "--dto", "-d" },"dto project directory，default ./Share");
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
