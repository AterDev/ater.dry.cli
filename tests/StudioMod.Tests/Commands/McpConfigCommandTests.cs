using CommandLine.Commands;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace CoreMod.Tests.Commands;

public class AgentInitCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateMcpJson_WhenFileDoesNotExist()
    {
        string tempDirectory = CreateTempDirectory();
        string originalCurrentDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tempDirectory);
            var command = new AgentInitCommand();

            var code = await command.ExecuteAsync(null!, CancellationToken.None);
            string configFilePath = Path.Combine(tempDirectory, ".vscode", "mcp.json");

            Assert.Equal(0, code);
            Assert.True(File.Exists(configFilePath));

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
    public async Task ExecuteAsync_ShouldAppendServerToExistingSecondLevelObject()
    {
        string tempDirectory = CreateTempDirectory();
        string originalCurrentDirectory = Directory.GetCurrentDirectory();
        string vscodeDirectory = Path.Combine(tempDirectory, ".vscode");
        string configFilePath = Path.Combine(vscodeDirectory, "mcp.json");

        Directory.CreateDirectory(vscodeDirectory);
        var root = new JsonObject
        {
            ["customContainer"] = new JsonObject
            {
                ["existing"] = new JsonObject
                {
                    ["command"] = "demo",
                    ["args"] = new JsonArray("serve"),
                },
            },
        };
        File.WriteAllText(configFilePath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        try
        {
            Directory.SetCurrentDirectory(tempDirectory);
            var command = new AgentInitCommand();

            var code = await command.ExecuteAsync(null!, CancellationToken.None);

            Assert.Equal(0, code);

            using var doc = JsonDocument.Parse(File.ReadAllText(configFilePath));
            var updatedRoot = doc.RootElement;
            var container = updatedRoot.GetProperty("customContainer");

            Assert.True(container.TryGetProperty("existing", out var existingServer));
            Assert.Equal("demo", existingServer.GetProperty("command").GetString());

            Assert.True(container.TryGetProperty("perigon", out var perigonServer));
            Assert.Equal("perigon", perigonServer.GetProperty("command").GetString());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCurrentDirectory);
            DeleteDirectoryIfExists(tempDirectory);
        }
    }

    private static string CreateTempDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"perigon-mcp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
