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
    }

    public RootCommand Build()
    {
        AddConfig();
        AddDto();
        AddApi();
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
        outputOption.SetDefaultValue(ConfigOptions.DtoPath);
        var forceOption = new Option<bool>(new[] { "--force", "-f" },
            "force overwrite file");
        forceOption.SetDefaultValue(false);

        dtoCommand.AddArgument(path);
        dtoCommand.AddOption(outputOption);
        dtoCommand.AddOption(forceOption);
        dtoCommand.SetHandler(async (string entity, string output, bool force) =>
        {
            await executor.GenerateDtoAsync(entity, output, force);
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
        dtoOption.SetDefaultValue(ConfigOptions.DtoPath);
        var storeOption = new Option<string>(new[] { "--datastore", "-s" },
            "dataStore project directory，default ./Http.Application");
        storeOption.SetDefaultValue(ConfigOptions.StorePath);
        var apiOption = new Option<string>(new[] { "--output", "-o" },
            "api controller project directory，default ./Http.API");
        apiOption.SetDefaultValue(ConfigOptions.ApiPath);
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
            await executor.GenerateApi(entity, dto, store, output);
        }, path, dtoOption, storeOption, apiOption, contextOption);

        RootCommand.Add(apiCommand);
    }

}
