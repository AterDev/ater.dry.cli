using CommandLine.Commands;
using Microsoft.Extensions.Localization;
using Moq;
using Share;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace CoreMod.Tests.Commands;

public class AgentInitCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateMcpJsonUnderAgents_WhenFileDoesNotExist()
    {
        string tempDirectory = CreateTempDirectory();
        string originalCurrentDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tempDirectory);
            var command = new AgentInitCommand(CreateLocalizer());

            var code = await command.ExecuteAsync(null!, CancellationToken.None);
            string configFilePath = Path.Combine(tempDirectory, ".agents", "mcp.json");

            Assert.Equal(0, code);
            Assert.True(File.Exists(configFilePath));
            Assert.False(File.Exists(Path.Combine(tempDirectory, ".vscode", "mcp.json")));

            using var doc = JsonDocument.Parse(File.ReadAllText(configFilePath));
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("servers", out var servers));
            Assert.True(servers.TryGetProperty("perigon", out var perigonServer));
            Assert.Equal("perigon", perigonServer.GetProperty("command").GetString());

            var args = perigonServer.GetProperty("args").EnumerateArray().Select(v => v.GetString()).ToList();
            Assert.Equal(["agent", "mcp"], args);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            DeleteDirectoryIfExists(tempDirectory);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreferAgentsMcpJsonOverLegacyVscodeConfig()
    {
        string tempDirectory = CreateTempDirectory();
        string originalCurrentDirectory = Directory.GetCurrentDirectory();
        string agentsConfigPath = Path.Combine(tempDirectory, ".agents", "mcp.json");
        string legacyConfigPath = Path.Combine(tempDirectory, ".vscode", "mcp.json");
        string settingsFilePath = Path.Combine(tempDirectory, ".vscode", "settings.json");

        WriteJson(agentsConfigPath, new JsonObject
        {
            ["servers"] = new JsonObject
            {
                ["agentsServer"] = new JsonObject
                {
                    ["command"] = "agents-command"
                }
            }
        });
        WriteJson(legacyConfigPath, new JsonObject
        {
            ["servers"] = new JsonObject
            {
                ["legacyServer"] = new JsonObject
                {
                    ["command"] = "legacy-command"
                }
            }
        });
        WriteText(settingsFilePath, """
            {
                // Keep existing JSONC settings readable.
                "chat.useAgentsMdFile": true,
            }
            """);

        try
        {
            Directory.SetCurrentDirectory(tempDirectory);
            var command = new AgentInitCommand(CreateLocalizer());

            var code = await command.ExecuteAsync(null!, CancellationToken.None);

            Assert.Equal(0, code);
            using var doc = JsonDocument.Parse(File.ReadAllText(agentsConfigPath));
            var servers = doc.RootElement.GetProperty("servers");
            Assert.Equal("agents-command", servers.GetProperty("agentsServer").GetProperty("command").GetString());
            Assert.True(servers.TryGetProperty("perigon", out _));
            Assert.False(servers.TryGetProperty("legacyServer", out _));

            using var settingsDoc = JsonDocument.Parse(File.ReadAllText(settingsFilePath));
            Assert.True(settingsDoc.RootElement.GetProperty("chat.mcp.discovery.enabled").GetBoolean());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            DeleteDirectoryIfExists(tempDirectory);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMigrateLegacyVscodeMcpConfig_WhenAgentsConfigDoesNotExist()
    {
        string tempDirectory = CreateTempDirectory();
        string originalCurrentDirectory = Directory.GetCurrentDirectory();
        string legacyConfigPath = Path.Combine(tempDirectory, ".vscode", "mcp.json");

        WriteJson(legacyConfigPath, new JsonObject
        {
            ["servers"] = new JsonObject
            {
                ["legacyServer"] = new JsonObject
                {
                    ["command"] = "legacy-command"
                }
            }
        });

        try
        {
            Directory.SetCurrentDirectory(tempDirectory);
            var command = new AgentInitCommand(CreateLocalizer());

            var code = await command.ExecuteAsync(null!, CancellationToken.None);
            string agentsConfigPath = Path.Combine(tempDirectory, ".agents", "mcp.json");

            Assert.Equal(0, code);
            using var doc = JsonDocument.Parse(File.ReadAllText(agentsConfigPath));
            var servers = doc.RootElement.GetProperty("servers");
            Assert.True(servers.TryGetProperty("legacyServer", out _));
            Assert.True(servers.TryGetProperty("perigon", out _));

            using var legacyDoc = JsonDocument.Parse(File.ReadAllText(legacyConfigPath));
            Assert.False(legacyDoc.RootElement.GetProperty("servers").TryGetProperty("perigon", out _));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            DeleteDirectoryIfExists(tempDirectory);
        }
    }

    [Fact]
    public void ApplyPerigonSkill_ShouldExtractOnlyPerigonSkill()
    {
        string tempDirectory = CreateTempDirectory();
        try
        {
            CreateAgentArchive(tempDirectory,
                (".agents/skills/perigon/SKILL.md", "perigon-v2"),
                (".agents/skills/code-review/SKILL.md", "code-review"),
                ("docs/Development/ProjectTracking.md", "tracking"));

            AgentInitCommand.ApplyPerigonSkill(tempDirectory);

            Assert.Equal(
                "perigon-v2",
                File.ReadAllText(Path.Combine(tempDirectory, ".agents", "skills", "perigon", "SKILL.md")));
            Assert.False(File.Exists(Path.Combine(tempDirectory, ".agents", "skills", "code-review", "SKILL.md")));
            Assert.False(File.Exists(Path.Combine(tempDirectory, "docs", "Development", "ProjectTracking.md")));
        }
        finally
        {
            DeleteDirectoryIfExists(tempDirectory);
        }
    }

    [Fact]
    public void ApplyLoopSpec_ShouldExtractSpecifiedSkillsAndPreserveExistingDocs()
    {
        string tempDirectory = CreateTempDirectory();
        string existingTrackingPath = Path.Combine(tempDirectory, "docs", "Development", "ProjectTracking.md");

        try
        {
            WriteText(existingTrackingPath, "project-owned tracking");
            CreateAgentArchive(tempDirectory,
                (".agents/skills/code-review/SKILL.md", "code-review"),
                (".agents/skills/commit-message/SKILL.md", "commit-message"),
                (".agents/skills/delivery-loop/SKILL.md", "delivery-loop"),
                (".agents/skills/docs/SKILL.md", "docs-skill"),
                (".agents/skills/test/SKILL.md", "test"),
                (".agents/skills/perigon/SKILL.md", "perigon"),
                ("docs/Development/ProjectTracking.md", "template tracking"),
                ("docs/UserStory/Demand.md", "template demand"));

            AgentInitCommand.ApplyLoopSpec(tempDirectory);

            Assert.Equal("code-review", ReadExtractedFile(tempDirectory, ".agents/skills/code-review/SKILL.md"));
            Assert.Equal("commit-message", ReadExtractedFile(tempDirectory, ".agents/skills/commit-message/SKILL.md"));
            Assert.Equal("delivery-loop", ReadExtractedFile(tempDirectory, ".agents/skills/delivery-loop/SKILL.md"));
            Assert.Equal("docs-skill", ReadExtractedFile(tempDirectory, ".agents/skills/docs/SKILL.md"));
            Assert.Equal("test", ReadExtractedFile(tempDirectory, ".agents/skills/test/SKILL.md"));
            Assert.False(File.Exists(Path.Combine(tempDirectory, ".agents", "skills", "perigon", "SKILL.md")));
            Assert.Equal("project-owned tracking", File.ReadAllText(existingTrackingPath));
            Assert.Equal("template demand", ReadExtractedFile(tempDirectory, "docs/UserStory/Demand.md"));
        }
        finally
        {
            DeleteDirectoryIfExists(tempDirectory);
        }
    }

    private static string CreateTempDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"perigon-agent-init-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void CreateAgentArchive(string directory, params (string Path, string Content)[] files)
    {
        string archivePath = Path.Combine(directory, "agent.zip");
        using ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);

        foreach ((string path, string content) in files)
        {
            ZipArchiveEntry entry = archive.CreateEntry(path);
            using StreamWriter writer = new(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write(content);
        }
    }

    private static string ReadExtractedFile(string directory, string relativePath) =>
        File.ReadAllText(Path.Combine(directory, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static void WriteJson(string path, JsonObject content)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void WriteText(string path, string content)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);
    }

    private static Localizer CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<Localizer>>();
        localizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] _) => new LocalizedString(name, name));
        return new Localizer(localizer.Object);
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
