using CoreMod.Services;
using Share;
using Share.Helper;
using System.ComponentModel;

namespace CommandLine.Commands;

/// <summary>
/// Pack module command
/// </summary>
public class PackCommand(
    ModulePackageService modulePackageService,
    SolutionContext projectContext
) : AsyncCommand<PackCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<ModuleName>")]
        [Description("Module name (with Mod suffix)")]
        public required string ModuleName { get; set; }

        [CommandArgument(1, "<ServiceName>")]
        [Description("Service name in Services directory")]
        public required string ServiceName { get; set; }

        [CommandOption("--front-path <FRONT_PATH>")]
        [Description("Frontend directory to include in the package")]
        public string? FrontPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        var currentDirectory = Environment.CurrentDirectory;
        await projectContext.SetSolutionAsync(currentDirectory);
        // Ensure we have a valid project context
        if (string.IsNullOrEmpty(projectContext.SolutionPath))
        {
            OutputHelper.Error("Error: Not in a valid solution directory");
            return 1;
        }

        var packagePath = await modulePackageService.PackageModuleAsync(
            settings.ModuleName,
            settings.ServiceName,
            settings.FrontPath
        );

        return packagePath != null ? 0 : 1;
    }
}
