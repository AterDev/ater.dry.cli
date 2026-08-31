using Share;
using Share.Helper;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CommandLine.Commands;

public class AgentInitCommand(Localizer localizer) : AsyncCommand
{
    private const string PerigonMcpAction = "Perigon MCP";
    private const string PerigonSkillAction = "Perigon Skill";
    private const string LoopSpecAction = "Loop spec";
    private const string McpDiscoverySetting = "chat.mcp.discovery.enabled";

    private static readonly JsonSerializerOptions McpJsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    private static readonly ZipExtractionRule[] PerigonSkillEntries =
    [
        new(".agents/skills/perigon", OverwriteFiles: true)
    ];

    private static readonly ZipExtractionRule[] LoopSpecEntries =
    [
        new(".agents/skills/code-review", OverwriteFiles: true),
        new(".agents/skills/commit-message", OverwriteFiles: true),
        new(".agents/skills/delivery-loop", OverwriteFiles: true),
        new(".agents/skills/docs", OverwriteFiles: true),
        new(".agents/skills/test", OverwriteFiles: true),
        new("docs", OverwriteFiles: false)
    ];

    private sealed record ZipExtractionRule(string RootPath, bool OverwriteFiles);

    public override Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string currentDirectory = Directory.GetCurrentDirectory();

        IReadOnlyList<string> selectedActions;
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            selectedActions = [PerigonMcpAction];
        }
        else
        {
            selectedActions = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title(localizer.Get(Localizer.AgentInitSelectActions))
                    .AddChoices(GetAgentSetupChoices(localizer))
                    .NotRequired()
            );
        }

        if (selectedActions.Count == 0)
        {
            OutputHelper.Success("No agent setup actions were selected.");
            return Task.FromResult(0);
        }

        try
        {
            if (IsActionSelected(selectedActions, PerigonMcpAction))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyMcpConfig(currentDirectory);
            }

            if (IsActionSelected(selectedActions, PerigonSkillAction))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyPerigonSkill(currentDirectory);
            }

            if (IsActionSelected(selectedActions, LoopSpecAction))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyLoopSpec(currentDirectory);
            }

            return Task.FromResult(0);
        }
        catch (JsonException ex)
        {
            OutputHelper.Error($"Failed to parse agent configuration: {ex.Message}");
            return Task.FromResult(-1);
        }
        catch (InvalidOperationException ex)
        {
            OutputHelper.Error(ex.Message);
            return Task.FromResult(-1);
        }
        catch (InvalidDataException ex)
        {
            OutputHelper.Error($"Invalid agent archive: {ex.Message}");
            return Task.FromResult(-1);
        }
        catch (IOException ex)
        {
            OutputHelper.Error($"Failed to update agent setup: {ex.Message}");
            return Task.FromResult(-1);
        }
    }

    private static void ApplyMcpConfig(string currentDirectory)
    {
        string configFilePath = Path.Combine(currentDirectory, ".agents", "mcp.json");
        string configDirectory = Path.GetDirectoryName(configFilePath) ?? currentDirectory;
        Directory.CreateDirectory(configDirectory);

        JsonObject root = LoadMcpConfigRoot(currentDirectory);
        JsonObject serverContainer = GetOrCreateServerContainer(root);
        serverContainer[ConstVal.CommandName] = CreateServerConfig();

        File.WriteAllText(configFilePath, root.ToJsonString(McpJsonSerializerOptions));
        EnsureVsCodeMcpDiscoveryEnabled(currentDirectory);
        OutputHelper.Success($"MCP server config has been written to {configFilePath}");
    }

    internal static void ApplyPerigonSkill(string currentDirectory)
    {
        ExtractAgentEntries(currentDirectory, PerigonSkillEntries, "Perigon Skill");
    }

    internal static void ApplyLoopSpec(string currentDirectory)
    {
        ExtractAgentEntries(currentDirectory, LoopSpecEntries, "Loop spec");
    }

    private static string[] GetAgentSetupChoices(Localizer localizer) =>
    [
        $"{PerigonMcpAction} - {localizer.Get(Localizer.AgentInitMcpChoiceDescription)}",
        $"{PerigonSkillAction} - {localizer.Get(Localizer.AgentInitSkillChoiceDescription)}",
        $"{LoopSpecAction} - {localizer.Get(Localizer.AgentInitLoopSpecChoiceDescription)}"
    ];

    private static bool IsActionSelected(IEnumerable<string> selectedActions, string actionName) =>
        selectedActions.Any(action => action.StartsWith(actionName, StringComparison.OrdinalIgnoreCase));

    private static void ExtractAgentEntries(
        string currentDirectory,
        IReadOnlyList<ZipExtractionRule> extractionRules,
        string setupName
    )
    {
        string? agentZipPath = FindAgentZipPath(currentDirectory);
        if (string.IsNullOrWhiteSpace(agentZipPath))
        {
            OutputHelper.Success("未找到 agent.zip");
            return;
        }

        using ZipArchive archive = ZipFile.OpenRead(agentZipPath);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string entryPath = NormalizeZipEntryPath(entry.FullName);
            if (string.IsNullOrEmpty(entryPath))
            {
                continue;
            }

            ZipExtractionRule? matchingRule = extractionRules.FirstOrDefault(rule =>
                IsPathUnderRoot(entryPath, rule.RootPath));
            if (matchingRule is null)
            {
                continue;
            }

            string destinationPath = GetSafeDestinationPath(currentDirectory, entryPath);
            bool isDirectory = entry.FullName.EndsWith("/", StringComparison.Ordinal)
                || entry.FullName.EndsWith("\\", StringComparison.Ordinal);

            if (isDirectory)
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            if (!matchingRule.OverwriteFiles && File.Exists(destinationPath))
            {
                continue;
            }

            string destinationDirectory = Path.GetDirectoryName(destinationPath) ?? currentDirectory;
            Directory.CreateDirectory(destinationDirectory);
            entry.ExtractToFile(destinationPath, matchingRule.OverwriteFiles);
        }

        OutputHelper.Success($"Extracted {setupName} from {agentZipPath} to {currentDirectory}");
    }

    private static string? FindAgentZipPath(string currentDirectory)
    {
        HashSet<string> candidates = [];

        foreach (string path in new[]
        {
            Path.Combine(currentDirectory, "agent.zip"),
            Path.Combine(currentDirectory, "src", "Apps", "CommandLine", "agent.zip"),
            Path.Combine(AppContext.BaseDirectory, "agent.zip")
        })
        {
            candidates.Add(Path.GetFullPath(path));
        }

        for (DirectoryInfo? directory = new(currentDirectory); directory is not null; directory = directory.Parent)
        {
            candidates.Add(Path.GetFullPath(Path.Combine(directory.FullName, "agent.zip")));
            candidates.Add(Path.GetFullPath(Path.Combine(directory.FullName, "src", "Apps", "CommandLine", "agent.zip")));
        }

        return candidates.FirstOrDefault(File.Exists);
    }

    private static JsonObject LoadMcpConfigRoot(string currentDirectory)
    {
        string agentsConfigPath = Path.Combine(currentDirectory, ".agents", "mcp.json");
        if (File.Exists(agentsConfigPath))
        {
            return LoadConfigRoot(agentsConfigPath);
        }

        string legacyConfigPath = Path.Combine(currentDirectory, ".vscode", "mcp.json");
        return LoadConfigRoot(File.Exists(legacyConfigPath) ? legacyConfigPath : agentsConfigPath);
    }

    private static void EnsureVsCodeMcpDiscoveryEnabled(string currentDirectory)
    {
        string vscodeDirectory = Path.Combine(currentDirectory, ".vscode");
        if (!Directory.Exists(vscodeDirectory))
        {
            return;
        }

        string settingsFilePath = Path.Combine(vscodeDirectory, "settings.json");
        JsonObject settings = LoadConfigRoot(settingsFilePath);
        settings[McpDiscoverySetting] = true;
        File.WriteAllText(settingsFilePath, settings.ToJsonString(McpJsonSerializerOptions));
    }

    private static bool IsPathUnderRoot(string path, string rootPath) =>
        path.Equals(rootPath, StringComparison.OrdinalIgnoreCase)
        || path.StartsWith($"{rootPath}/", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeZipEntryPath(string entryPath)
    {
        if (string.IsNullOrWhiteSpace(entryPath))
        {
            return string.Empty;
        }

        if (entryPath.StartsWith("/", StringComparison.Ordinal)
            || entryPath.StartsWith("\\", StringComparison.Ordinal)
            || Path.IsPathRooted(entryPath))
        {
            throw new InvalidDataException($"The agent archive contains an unsafe path: {entryPath}");
        }

        string[] segments = entryPath
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Any(segment => segment is "." or ".." || segment.Contains(':')))
        {
            throw new InvalidDataException($"The agent archive contains an unsafe path: {entryPath}");
        }

        return string.Join('/', segments);
    }

    private static string GetSafeDestinationPath(string currentDirectory, string relativePath)
    {
        string rootPath = Path.GetFullPath(currentDirectory);
        string destinationPath = Path.GetFullPath(
            Path.Combine(rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        string rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!destinationPath.Equals(rootPath, comparison)
            && !destinationPath.StartsWith(rootPrefix, comparison))
        {
            throw new InvalidDataException($"The agent archive contains an unsafe path: {relativePath}");
        }

        return destinationPath;
    }

    internal static JsonObject LoadConfigRoot(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            return [];
        }

        string content = File.ReadAllText(configFilePath);
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        JsonNode? node = JsonNode.Parse(
            content,
            documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            }
        );

        return node as JsonObject
            ?? throw new InvalidOperationException($"The root node of {configFilePath} must be a JSON object.");
    }

    internal static JsonObject GetOrCreateServerContainer(JsonObject root)
    {
        if (root["servers"] is JsonObject existingServerContainer)
        {
            return existingServerContainer;
        }

        foreach ((_, JsonNode? value) in root)
        {
            if (value is JsonObject childObject)
            {
                return childObject;
            }
        }

        JsonObject serverContainer = [];
        root["servers"] = serverContainer;
        return serverContainer;
    }

    internal static JsonObject CreateServerConfig() =>
        new()
        {
            ["command"] = ConstVal.CommandName,
            ["args"] = new JsonArray(SubCommand.Agent, SubCommand.Mcp),
        };
}
