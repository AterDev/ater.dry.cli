using System.CommandLine;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Share.Services;

namespace CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0) return 0;
        Console.OutputEncoding = Encoding.UTF8;
        ShowLogo();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var commandBuilder = serviceProvider.GetRequiredService<CommandBuilder>();
        var root = commandBuilder.Build();
        return await root.InvokeAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(config => { config.AddConsole(); })
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

        services.AddSingleton<CodeAnalysisService>();
        services.AddSingleton<CodeGenService>();
        services.AddSingleton<CommandRunner>();
        services.AddSingleton<CommandBuilder>();
        services.AddSingleton<StudioCommand>();
    }

    private static void ShowLogo()
    {
        string logo = """
                 _____    _____   __     __
                |  __ \  |  __ \  \ \   / /
                | |  | | | |__) |  \ \_/ / 
                | |  | | |  _  /    \   /  
                | |__| | | | \ \     | |   
                |_____/  |_|  \_\    |_|

                       —→ for freedom 🗽 ←—

            """;
        Console.WriteLine(logo);
    }
}
