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

        [CommandOption("-l|--list")]
        [Description("List official modules")]
        public bool List { get; set; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken
    )
    {
        if (
            settings.List
            || string.Equals(settings.PackagePath, "list", StringComparison.OrdinalIgnoreCase)
        )
        {
            return await ShowOfficialModulesAsync(cancellationToken);
        }

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
            cancellationToken
        );

        return success ? 0 : 1;
    }

    private async Task<int> ShowOfficialModulesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var modules = await officialModuleService.GetOfficialModulesAsync(cancellationToken);
            if (modules.Count == 0)
            {
                OutputHelper.Warning(localizer.Get(Localizer.OfficialModuleListEmpty));
                return 0;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.Title = new TableTitle(localizer.Get(Localizer.OfficialModuleListTitle));
            table.AddColumn(localizer.Get(Localizer.Name));
            table.AddColumn(localizer.Get(Localizer.Version));
            table.AddColumn(localizer.Get(Localizer.Author));
            table.AddColumn(localizer.Get(Localizer.Description));

            foreach (var module in modules)
            {
                table.AddRow(
                    Markup.Escape($"Perigon.{module.ModuleName}"),
                    Markup.Escape(module.Version),
                    Markup.Escape(module.Author ?? "-"),
                    Markup.Escape(module.Description ?? "-")
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
        catch (Exception ex)
        {
            OutputHelper.Error(ex.Message);
            return 1;
        }
    }
}
