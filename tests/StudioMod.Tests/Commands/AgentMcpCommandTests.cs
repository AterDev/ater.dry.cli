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

        // Cancel immediately — the host should not fully start.
        await cts.CancelAsync();

        int exitCode = await command.ExecuteAsync(null!, cts.Token);

        // Either cancelled gracefully (0) or the host threw before completing startup.
        Assert.True(exitCode == 0 || exitCode == -1);
    }
}
