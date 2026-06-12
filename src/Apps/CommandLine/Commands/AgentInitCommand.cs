using Share;
using Share.Helper;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CommandLine.Commands;

public class AgentInitCommand : AsyncCommand
{
    private static readonly JsonSerializerOptions McpJsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public override Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string currentDirectory = Directory.GetCurrentDirectory();

        IReadOnlyList<string> selectedActions;
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            selectedActions = ["MCP"];
        }
        else
        {
            selectedActions = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select agent setup actions")
                    .AddChoices(["MCP", "Skills"])
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
            if (selectedActions.Contains("MCP", StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplyMcpConfig(currentDirectory);
            }

            if (selectedActions.Contains("Skills", StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ApplySkills(currentDirectory);
            }

            return Task.FromResult(0);
        }
        catch (JsonException ex)
        {
            OutputHelper.Error($"Failed to parse .vscode/mcp.json: {ex.Message}");
            return Task.FromResult(-1);
        }
        catch (InvalidOperationException ex)
        {
            OutputHelper.Error(ex.Message);
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
        string configFilePath = Path.Combine(currentDirectory, ".vscode", "mcp.json");
        string configDirectory = Path.GetDirectoryName(configFilePath) ?? currentDirectory;
        Directory.CreateDirectory(configDirectory);

        JsonObject root = LoadConfigRoot(configFilePath);
        JsonObject serverContainer = GetOrCreateServerContainer(root);
        serverContainer[ConstVal.CommandName] = CreateServerConfig();

        File.WriteAllText(configFilePath, root.ToJsonString(McpJsonSerializerOptions));
        OutputHelper.Success($"MCP server config has been written to {configFilePath}");
    }

    private static void ApplySkills(string currentDirectory)
    {
        string? agentZipPath = FindAgentZipPath(currentDirectory);
        if (string.IsNullOrWhiteSpace(agentZipPath))
        {
            OutputHelper.Success("未找到 agent.zip");
            return;
        }

        ZipFile.ExtractToDirectory(agentZipPath, currentDirectory, overwriteFiles: true);
        OutputHelper.Success($"Extracted agent skills from {agentZipPath} to {currentDirectory}");
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
            ?? throw new InvalidOperationException("The root node of .vscode/mcp.json must be a JSON object.");
    }

    internal static JsonObject GetOrCreateServerContainer(JsonObject root)
    {
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
