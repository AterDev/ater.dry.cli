using CoreMod.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.JavaScript.NodeApi;
using Share;

namespace Perigon.NodeBridge;

/// <summary>
/// Node bridge for request client generation.
/// </summary>
public static class RequestBridge
{
    private static readonly string[] SupportedTypes = ["angular", "axios", "csharp"];

    [JSExport]
    public static async Task<int> Request(
        string path,
        string outputPath,
        string type = "angular",
        bool onlyModel = false
    )
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(outputPath))
        {
            Console.Error.WriteLine("Both path and outputPath are required.");
            return -1;
        }

        if (!TryMapRequestType(type, out var requestClientType))
        {
            Console.Error.WriteLine("Invalid type, only support: csharp, angular, axios");
            return -1;
        }

        using var host = CreateHost();
        using var scope = host.Services.CreateScope();
        var commandService = scope.ServiceProvider.GetRequiredService<CommandService>();

        await commandService.GenerateRequestClientAsync(path, outputPath, requestClientType, onlyModel);
        return 0;
    }

    [JSExport]
    public static string[] GetSupportedRequestTypes() => SupportedTypes;

    private static IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.AddConsole();

        builder.AddFrameworkServices();
        builder.Services.AddScoped<SolutionContext>();
        builder.Services.AddScoped<SolutionService>();
        builder.Services.AddScoped<CodeGenService>();
        builder.Services.AddScoped<CommandService>();

        return builder.Build();
    }

    private static bool TryMapRequestType(string? input, out RequestClientType type)
    {
        type = RequestClientType.NgHttp;

        if (input is null)
        {
            return false;
        }

        return input.Trim().ToLowerInvariant() switch
        {
            "angular" => SetType(RequestClientType.NgHttp, out type),
            "axios" => SetType(RequestClientType.Axios, out type),
            "csharp" => SetType(RequestClientType.CSharp, out type),
            _ => false,
        };
    }

    private static bool SetType(RequestClientType value, out RequestClientType type)
    {
        type = value;
        return true;
    }
}
