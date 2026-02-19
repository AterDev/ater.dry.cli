using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Share;
using Share.Helper;
using Share.Services;
using System.Text.Json;
using CoreMod.Managers;
using CoreMod.McpTools;
using CoreMod.Services;

namespace CommandLine.Commands;

public class McpConfigCommand : AsyncCommand
{
    public override Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        var config = new
        {
            mcpServers = new Dictionary<string, object>
            {
                [ConstVal.CommandName] = new
                {
                    command = ConstVal.CommandName,
                    args = new[] { SubCommand.Mcp, SubCommand.Start }
                }
            }
        };

        Console.WriteLine(
            JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            )
        );

        return Task.FromResult(0);
    }
}

public class McpStartCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        Environment.SetEnvironmentVariable("PERIGON_MCP_STDIO", "1");

        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        builder.AddFrameworkServices();
        builder.Services.AddLocalization();
        builder.Services.AddScoped<Localizer>();
        builder.Services.AddScoped<IProjectContext, ProjectContext>();
        builder.Services.AddScoped<SolutionService>();
        builder.Services.AddScoped<CodeAnalysisService>();
        builder.Services.AddScoped<CodeGenService>();
        builder.Services.AddScoped<CommandService>();
        builder.Services.AddScoped<EntityInfoManager>();
        builder.Services.AddScoped<GenActionManager>();
        builder.Services.AddScoped<ActionRunModelService>();

        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly(typeof(CodeTools).Assembly);

        using var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILogger<McpStartCommand>>();
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

        using var ctRegistration = cancellationToken.Register(() =>
            logger.LogInformation("MCP cancellation requested from command token.")
        );
        using var startedRegistration = lifetime.ApplicationStarted.Register(() =>
            logger.LogInformation(
                "MCP server started over stdio. PID={Pid}, ToolsAssembly={AssemblyName}",
                Environment.ProcessId,
                typeof(CodeTools).Assembly.GetName().Name
            )
        );
        using var stoppingRegistration = lifetime.ApplicationStopping.Register(() =>
            logger.LogInformation("MCP server stopping...")
        );
        using var stoppedRegistration = lifetime.ApplicationStopped.Register(() =>
            logger.LogInformation("MCP server stopped.")
        );

        try
        {
            logger.LogInformation("Starting MCP server with stdio transport...");
            await host.RunAsync(cancellationToken);
            logger.LogInformation("MCP host run loop exited normally.");
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("MCP host run loop canceled by caller.");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MCP server terminated unexpectedly.");
            return -1;
        }
    }
}