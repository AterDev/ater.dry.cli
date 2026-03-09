using CoreMod.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Share;
using System.Net;
using System.Text;
using Xunit;

namespace CoreMod.Tests.Services;

public class OfficialModuleServiceTests
{
    [Theory]
    [InlineData("Perigon.SystemMod", true, "SystemMod")]
    [InlineData("Perigon.SystemMod.zip", true, "SystemMod")]
    [InlineData("SystemMod.zip", false, "SystemMod")]
    public void OfficialPackageHelpers_ShouldRecognizeAndNormalize(
        string packageName,
        bool isOfficial,
        string expectedModuleName
    )
    {
        Assert.Equal(isOfficial, OfficialModuleService.IsOfficialPackageName(packageName));
        Assert.Equal(expectedModuleName, OfficialModuleService.NormalizeOfficialModuleName(packageName));
    }

    [Fact]
    public async Task GetOfficialModulesAsync_ShouldDeserializeAndSortModules()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var service = CreateService(
            new StubHttpMessageHandler(request =>
            {
                if (request.RequestUri?.Host == "api.github.com")
                {
                    if (request.RequestUri.AbsoluteUri == "https://api.github.com/")
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                    }

                    if (request.RequestUri.AbsoluteUri.Contains("/git/trees/main?recursive=1"))
                    {
                        var treeJson = """
                            {
                              "tree": [
                                {
                                                                    "path": "modules.json",
                                  "url": "https://api.github.com/repos/AterDev/Perigon.Modules/git/blobs/modules-json-sha"
                                }
                              ]
                            }
                            """;

                        return Task.FromResult(
                            new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(treeJson, Encoding.UTF8, "application/json")
                            }
                        );
                    }

                    if (request.RequestUri.AbsoluteUri.Contains("/git/blobs/modules-json-sha"))
                    {
                        var blobJson = """
                            {
                              "content": "WwogIHsKICAgICJNb2R1bGVOYW1lIjogIlN5c3RlbU1vZCIsCiAgICAiQXV0aG9yIjogIlBlcmlnb24iLAogICAgIkRpc3BsYXlOYW1lIjogIlN5c3RlbU1vZCIsCiAgICAiRGVzY3JpcHRpb24iOiAiU3lzdGVtIG1vZHVsZSIsCiAgICAiUGFja2FnZVR5cGUiOiAwLAogICAgIlZlcnNpb24iOiAiMS4wLjAiCiAgfSwKICB7CiAgICAiTW9kdWxlTmFtZSI6ICJDTVNNb2QiLAogICAgIkF1dGhvciI6ICJQZXJpZ29uIiwKICAgICJEaXNwbGF5TmFtZSI6ICJDTVNNb2QiLAogICAgIkRlc2NyaXB0aW9uIjogIkNNUyBtb2R1bGUiLAogICAgIlBhY2thZ2VUeXBlIjogMCwKICAgICJWZXJzaW9uIjogIjEuMC4xIgogIH0KXQ=="
                            }
                            """;

                        return Task.FromResult(
                            new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(blobJson, Encoding.UTF8, "application/json")
                            }
                        );
                    }
                }

                throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
            })
        );

        var modules = await service.GetOfficialModulesAsync(cancellationToken);

        Assert.Equal(2, modules.Count);
        Assert.Equal("CMSMod", modules[0].ModuleName);
        Assert.Equal("SystemMod", modules[1].ModuleName);
        Assert.Equal("1.0.1", modules[0].Version);
    }

    [Fact]
    public async Task DownloadOfficialModulePackageAsync_ShouldPersistZipFile()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var zipBytes = Encoding.UTF8.GetBytes("fake-zip-content");
        var service = CreateService(
            new StubHttpMessageHandler(request =>
            {
                if (request.RequestUri?.Host == "api.github.com")
                {
                    if (request.RequestUri.AbsoluteUri == "https://api.github.com/")
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                    }

                    if (request.RequestUri.AbsoluteUri.Contains("/git/trees/main?recursive=1"))
                    {
                        var treeJson = """
                            {
                              "tree": [
                                {
                                                                    "path": "modules.json",
                                  "url": "https://api.github.com/repos/AterDev/Perigon.Modules/git/blobs/modules-json-sha"
                                }
                              ]
                            }
                            """;

                        return Task.FromResult(
                            new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(treeJson, Encoding.UTF8, "application/json")
                            }
                        );
                    }

                    if (request.RequestUri.AbsoluteUri.Contains("/git/blobs/modules-json-sha"))
                    {
                        var blobJson = """
                            {
                              "content": "WwogIHsKICAgICJNb2R1bGVOYW1lIjogIlN5c3RlbU1vZCIsCiAgICAiQXV0aG9yIjogIlBlcmlnb24iLAogICAgIkRpc3BsYXlOYW1lIjogIlN5c3RlbU1vZCIsCiAgICAiRGVzY3JpcHRpb24iOiAiU3lzdGVtIG1vZHVsZSIsCiAgICAiUGFja2FnZVR5cGUiOiAwLAogICAgIlZlcnNpb24iOiAiMS4wLjAiCiAgfQpd"
                            }
                            """;

                        return Task.FromResult(
                            new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(blobJson, Encoding.UTF8, "application/json")
                            }
                        );
                    }
                }

                if (request.RequestUri?.AbsoluteUri.Contains("package_modules/SystemMod.zip") == true)
                {
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(zipBytes)
                        }
                    );
                }

                throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
            })
        );

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            var packagePath = await service.DownloadOfficialModulePackageAsync(
                "Perigon.SystemMod",
                tempDir,
                cancellationToken
            );

            Assert.True(File.Exists(packagePath));
            Assert.Equal(zipBytes, await File.ReadAllBytesAsync(packagePath, cancellationToken));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    private static OfficialModuleService CreateService(HttpMessageHandler handler)
    {
        var stringLocalizer = new Mock<IStringLocalizer<Localizer>>();
        stringLocalizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        stringLocalizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var localizer = new Localizer(stringLocalizer.Object);
        return new OfficialModuleService(
            new HttpClient(handler),
            localizer,
            NullLogger<OfficialModuleService>.Instance
        );
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            return handler(request);
        }
    }
}