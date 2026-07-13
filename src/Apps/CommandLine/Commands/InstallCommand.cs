using CoreMod.Services;
using Share;
using Share.Helper;
using System.ComponentModel;

namespace CommandLine.Commands;

/// <summary>
/// Install module command
/// </summary>
public class InstallCommand(
    ModuleInstallService moduleInstallService,
    OfficialModuleService officialModuleService,
    SolutionContext projectContext,
    Localizer localizer
) : AsyncCommand<InstallCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[PackagePath]")]
        [Description("Path to the module package zip file, or official package name like Perigon.SystemMod")]
        public string? PackagePath { get; set; }

        [CommandArgument(1, "[ServiceName]")]
        [Description("Service name in Services directory")]
        public string? ServiceName { get; set; }

        [CommandOption("--front-path <FRONT_PATH>")]
        [Description("Directory where bundled frontend code will be restored")]
        public string? FrontPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(settings.PackagePath) || string.IsNullOrWhiteSpace(settings.ServiceName))
        {
            OutputHelper.Error(localizer.Get(Localizer.InstallArgumentsRequired));
            return 1;
        }

        if (!await CommandSolutionHelper.TrySetSolutionAsync(projectContext, localizer))
        {
            return 1;
        }

        var success = await moduleInstallService.InstallModuleAsync(
            settings.PackagePath,
            settings.ServiceName,
            settings.FrontPath,
            cancellationToken
        );

        return success ? 0 : 1;
    }

    private async Task<int> ShowOfficialModulesAsync(CancellationToken cancellationToken)
    {
        return await ModuleCommandHelper.ShowOfficialModulesAsync(
            officialModuleService,
            localizer,
            cancellationToken
        );
    }
}
