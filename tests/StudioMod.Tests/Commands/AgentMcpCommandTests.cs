using CommandLine.Commands;
using Xunit;

namespace CoreMod.Tests.Commands;

public class AgentMcpCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldExitGracefully_WhenCancelledBeforeRun()
    {
        var command = new AgentMcpCommand();
        using var cts = new CancellationTokenSource();

        // Cancel immediately — the command should not build or start the host.
        await cts.CancelAsync();

        int exitCode = await command.ExecuteAsync(null!, cts.Token);

        Assert.Equal(0, exitCode);
    }
}
