using CoreMod.Managers;
using CoreMod.McpTools;
using CoreMod.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Share;
using Share.Helper;
using System.Text.Json;

namespace CommandLine.Commands;

public class McpConfigCommand : AsyncCommand
{
    public override Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var config = new Dictionary<string, object>
        {
            [ConstVal.CommandName] = new
            {
                command = ConstVal.CommandName,
                args = new[] { SubCommand.Mcp, SubCommand.Start }
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
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
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
        builder.Services.AddScoped<SolutionContext>();
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
            .WithToolsFromAssembly(typeof(MCPTools).Assembly);

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
                typeof(MCPTools).Assembly.GetName().Name
            )
        );

        try
        {
            await host.RunAsync(cancellationToken);
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