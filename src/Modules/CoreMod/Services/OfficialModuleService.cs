using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace CoreMod.Services;

/// <summary>
/// Provides access to official Perigon modules hosted on GitHub.
/// </summary>
public class OfficialModuleService
{
    private const string OfficialPackagePrefix = "Perigon.";
    private const string GitHubApiUrl = "https://api.github.com";
    private const string GitHubRepositoryTreeUrl =
        "https://api.github.com/repos/AterDev/Perigon.Modules/git/trees/main?recursive=1";
    private const string OfficialModulesMetadataPath = "modules.json";
    private const string OfficialModulesPackageUrlFormat =
        "https://raw.githubusercontent.com/AterDev/Perigon.Modules/main/package_modules/{0}.zip";

    private static readonly HttpClient SharedHttpClient = CreateDefaultHttpClient();
    private static readonly JsonSerializerOptions GitHubJsonSerializerOptions = new(
        ConstVal.DefaultJsonSerializerOptions
    )
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly Localizer _localizer;
    private readonly ILogger<OfficialModuleService> _logger;

    public OfficialModuleService(Localizer localizer, ILogger<OfficialModuleService> logger)
        : this(SharedHttpClient, localizer, logger) { }

    public OfficialModuleService(
        HttpClient httpClient,
        Localizer localizer,
        ILogger<OfficialModuleService> logger
    )
    {
        _httpClient = httpClient;
        _localizer = localizer;
        _logger = logger;
    }

    public static bool IsOfficialPackageName(string packageName)
    {
        return !string.IsNullOrWhiteSpace(packageName)
            && packageName.StartsWith(OfficialPackagePrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeOfficialModuleName(string packageName)
    {
        var moduleName = packageName.Trim();
        if (moduleName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            moduleName = Path.GetFileNameWithoutExtension(moduleName);
        }

        if (moduleName.StartsWith(OfficialPackagePrefix, StringComparison.OrdinalIgnoreCase))
        {
            moduleName = moduleName[OfficialPackagePrefix.Length..];
        }

        return moduleName;
    }

    public async Task<bool> CanConnectToGitHubAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GitHubApiUrl);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to GitHub");
            return false;
        }
    }

    public async Task<IReadOnlyList<PackageMetadata>> GetOfficialModulesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await EnsureGitHubConnectionAsync(cancellationToken);

        try
        {
            var metadataJson = await GetRepositoryTextContentAsync(
                OfficialModulesMetadataPath,
                cancellationToken
            );
            var modules = JsonSerializer.Deserialize<List<PackageMetadata>>(
                metadataJson,
                ConstVal.DefaultJsonSerializerOptions
            );

            return modules?
                .OrderBy(m => m.ModuleName, StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch official module metadata");
            throw new InvalidOperationException(
                _localizer.Get(Localizer.OfficialModulesFetchFailed),
                ex
            );
        }
    }

    public async Task<string> DownloadOfficialModulePackageAsync(
        string packageName,
        string targetDirectory,
        CancellationToken cancellationToken = default
    )
    {
        var moduleName = NormalizeOfficialModuleName(packageName);
        var modules = await GetOfficialModulesAsync(cancellationToken);
        var module = modules.FirstOrDefault(m =>
            string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase)
        );

        if (module == null)
        {
            throw new InvalidOperationException(
                _localizer.Get(Localizer.OfficialModuleNotFound, packageName)
            );
        }

        Directory.CreateDirectory(targetDirectory);
        var packagePath = Path.Combine(targetDirectory, $"{module.ModuleName}.zip");
        var downloadUrl = string.Format(OfficialModulesPackageUrlFormat, module.ModuleName);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

            await using var packageStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = File.Create(packagePath);
            await packageStream.CopyToAsync(fileStream, cancellationToken);

            return packagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download official module package {ModuleName}", moduleName);
            throw new InvalidOperationException(
                _localizer.Get(Localizer.OfficialModuleDownloadFailed, moduleName),
                ex
            );
        }
    }

    private async Task EnsureGitHubConnectionAsync(CancellationToken cancellationToken)
    {
        if (!await CanConnectToGitHubAsync(cancellationToken))
        {
            throw new InvalidOperationException(_localizer.Get(Localizer.GitHubConnectionFailed));
        }
    }

    private async Task<string> GetRepositoryTextContentAsync(
        string repositoryPath,
        CancellationToken cancellationToken
    )
    {
        var item = await GetRepositoryTreeItemAsync(repositoryPath, cancellationToken);
        if (string.IsNullOrWhiteSpace(item.Url))
        {
            throw new InvalidOperationException(_localizer.Get(Localizer.OfficialModulesFetchFailed));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, item.Url);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var blob = await JsonSerializer.DeserializeAsync<GitBlobResponse>(
            stream,
            GitHubJsonSerializerOptions,
            cancellationToken
        );

        if (string.IsNullOrWhiteSpace(blob?.Content))
        {
            throw new InvalidOperationException(_localizer.Get(Localizer.OfficialModulesFetchFailed));
        }

        var base64Content = blob.Content.Replace("\n", string.Empty).Replace("\r", string.Empty);
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64Content));
    }

    private async Task<GitTreeItem> GetRepositoryTreeItemAsync(
        string repositoryPath,
        CancellationToken cancellationToken
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GitHubRepositoryTreeUrl);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var tree = await JsonSerializer.DeserializeAsync<GitTreeResponse>(
            stream,
            GitHubJsonSerializerOptions,
            cancellationToken
        );

        var item = tree?.Tree.FirstOrDefault(x =>
            string.Equals(x.Path, repositoryPath, StringComparison.OrdinalIgnoreCase)
        );

        return item
            ?? throw new InvalidOperationException(_localizer.Get(Localizer.OfficialModulesFetchFailed));
    }

    private static HttpClient CreateDefaultHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("Perigon.CLI", AssemblyHelper.GetCurrentToolVersion())
        );
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/octet-stream")
        );
        return client;
    }

    private sealed class GitTreeResponse
    {
        [JsonPropertyName("tree")]
        public List<GitTreeItem> Tree { get; set; } = [];
    }

    private sealed class GitTreeItem
    {
        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private sealed class GitBlobResponse
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}