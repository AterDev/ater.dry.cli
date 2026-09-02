using CoreMod.Services;
using Share;
using Share.Helper;
using Share.Models;
using System.ComponentModel;

namespace CommandLine.Commands;

/// <summary>
/// Pack module command
/// </summary>
public class PackCommand(
    ModulePackageService modulePackageService,
    SolutionContext projectContext,
    Localizer localizer
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

        [CommandOption("-v|--version <VERSION>")]
        [Description("Package version; defaults to 1.0.0 when omitted / 包版本号；省略时默认使用 1.0.0")]
        public string? Version { get; set; }

        [CommandOption("--front-path <FRONT_PATH>")]
        [Description("Frontend module directory to include in the package")]
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

        var version = settings.Version;
        if (string.IsNullOrWhiteSpace(version))
        {
            version = PackageMetadata.DefaultVersion;
            OutputHelper.Warning(
                localizer.Get("PackageVersionDefaultWarning", version)
            );
        }

        var packagePath = await modulePackageService.PackageModuleAsync(
            settings.ModuleName,
            settings.ServiceName,
            settings.FrontPath,
            version
        );

        return packagePath != null ? 0 : 1;
    }
}
