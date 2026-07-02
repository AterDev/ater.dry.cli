using System.Net.Http.Headers;

namespace CoreMod.Services;

/// <summary>
/// Provides access to official Perigon modules hosted on GitHub.
/// </summary>
public class OfficialModuleService
{
    private const string OfficialPackagePrefix = "Perigon.";
    private const string GitHubApiUrl = "https://api.github.com";
    private static readonly TimeSpan OfficialModulesCacheDuration = TimeSpan.FromMinutes(1);
    private const string OfficialModulesMetadataUrl =
        "https://raw.githubusercontent.com/AterDev/Perigon.Modules/main/modules.json";
    private const string OfficialModulesPackageUrlFormat =
        "https://raw.githubusercontent.com/AterDev/Perigon.Modules/main/package_modules/{0}.zip";

    private static readonly HttpClient SharedHttpClient = CreateDefaultHttpClient();

    private readonly HttpClient _httpClient;
    private readonly Localizer _localizer;
    private readonly ILogger<OfficialModuleService> _logger;
    private readonly SemaphoreSlim _modulesCacheLock = new(1, 1);
    private IReadOnlyList<PackageMetadata>? _cachedOfficialModules;
    private DateTimeOffset _cachedOfficialModulesExpiresAt;

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
        if (_cachedOfficialModules is not null
            && _cachedOfficialModulesExpiresAt > DateTimeOffset.UtcNow)
        {
            return _cachedOfficialModules;
        }

        try
        {
            await _modulesCacheLock.WaitAsync(cancellationToken);
            try
            {
                if (_cachedOfficialModules is not null
                    && _cachedOfficialModulesExpiresAt > DateTimeOffset.UtcNow)
                {
                    return _cachedOfficialModules;
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, OfficialModulesMetadataUrl);
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var modules = await JsonSerializer.DeserializeAsync<List<PackageMetadata>>(
                    stream,
                    ConstVal.DefaultJsonSerializerOptions,
                    cancellationToken
                );

                _cachedOfficialModules = modules?
                    .OrderBy(m => m.ModuleName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    ?? [];
                _cachedOfficialModulesExpiresAt =
                    DateTimeOffset.UtcNow.Add(OfficialModulesCacheDuration);

                return _cachedOfficialModules;
            }
            finally
            {
                _modulesCacheLock.Release();
            }
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
}
