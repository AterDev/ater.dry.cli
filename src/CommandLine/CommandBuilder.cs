using System.CommandLine;

namespace Droplet.CommandLine;

public class CommandBuilder
{
    public RootCommand RootCommand { get; set; }

    public CommandBuilder()
    {
        RootCommand = new RootCommand("Welcome use droplet cli, a tool to help you generate source code!")
        {
            Name = "droplet"
        };
    }

    public RootCommand Build()
    {
        AddConfig();
        AddDto();
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
        dtoCommand.AddAlias("dto");

        var path = new Argument<string>("entity path", "The entity file path");
        var outputOption = new Option<string>(new[] { "--output", "-o" },
            "output project directory，default ./Share/Models");
        var forceOption = new Option<bool>(new[] { "--force" }, "force overwrite file");
        dtoCommand.AddArgument(path);
        dtoCommand.AddOption(outputOption);
        dtoCommand.AddOption(forceOption);
        dtoCommand.SetHandler((string entity, string output, bool force) =>
        {
            if (string.IsNullOrEmpty(output))
            {
                output = Config.DTO_PATH;
            }
            executor.GenerateDto(entity, output);
        }, path, outputOption, forceOption);

        RootCommand.Add(dtoCommand);
    }

}
