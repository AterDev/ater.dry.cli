using CodeGenerator.Generate;
using CodeGenerator.Generate.ClientRequest;
using CodeGenerator.Generate.LanguageFormatter;
using CodeGenerator.Models;
using Microsoft.OpenApi;
using Share.Models;
using Xunit;

namespace CoreMod.Tests.Generate;

public class RequestClientCompatibilityTests
{
    [Fact]
    public void NormalizeParameterName_ShouldConvertAndDeduplicate()
    {
        // Arrange
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Act
        var first = RequestClientHelper.NormalizeParameterName("api-version", used);
        var duplicate = RequestClientHelper.NormalizeParameterName("api version", used);
        var startsWithNumber = RequestClientHelper.NormalizeParameterName("123-name", used);
        var keyword = RequestClientHelper.NormalizeParameterName("class", used);

        // Assert
        Assert.Equal("apiVersion", first);
        Assert.Equal("apiVersion2", duplicate);
        Assert.Equal("arg123Name", startsWithNumber);
        Assert.Equal("classValue", keyword);
    }

    [Fact]
    public void CSharpFormatter_ShouldEmitJsonPropertyName_ForSpecialJsonKeys()
    {
        // Arrange
        var formatter = new CSharpFormatter();
        var meta = new TypeMeta
        {
            Name = "SampleDto",
            FullName = "Demo.SampleDto",
            Namespace = "Demo.Models",
            PropertyInfos =
            [
                new PropertyInfo { Name = "api-version", Type = "string" },
                new PropertyInfo { Name = "@id", Type = "string" },
                new PropertyInfo { Name = "normalName", Type = "string" },
            ]
        };

        // Act
        var code = formatter.GenerateModel(meta, "DemoClient");

        // Assert
        Assert.Contains("using System.Text.Json.Serialization;", code);
        Assert.Contains("[JsonPropertyName(\"api-version\")]", code);
        Assert.Contains("public string ApiVersion { get; set; } = default!;", code);
        Assert.Contains("[JsonPropertyName(\"@id\")]", code);
        Assert.Contains("public string Id { get; set; } = default!;", code);
        Assert.DoesNotContain("[JsonPropertyName(\"normalName\")]", code);
    }

    [Fact]
    public void TypeScriptFormatter_ShouldQuoteInvalidPropertyKeys()
    {
        // Arrange
        var formatter = new TypeScriptFormatter();
        var meta = new TypeMeta
        {
            Name = "SampleDto",
            FullName = "Demo.SampleDto",
            Namespace = "Demo.Models",
            PropertyInfos =
            [
                new PropertyInfo { Name = "@id", Type = "string" },
                new PropertyInfo { Name = "#text", Type = "string" },
                new PropertyInfo { Name = "normalName", Type = "string" },
            ]
        };

        // Act
        var code = formatter.GenerateModel(meta);

        // Assert
        Assert.Contains("'@id': string;", code);
        Assert.Contains("'#text': string;", code);
        Assert.Contains("normalName: string;", code);
    }

    [Fact]
    public void CSHttpClientGenerate_ShouldKeepOriginalQueryKey_AndUseSafeVariableName()
    {
        // Arrange
        var function = new RequestServiceFunction
        {
            Name = "User_Get",
            Tag = "User",
            Method = "Get",
            Path = "/users/{api-version}",
            ResponseType = "string",
            Params =
            [
                new FunctionParams
                {
                    Name = "apiVersion",
                    OriginalName = "api-version",
                    Type = "string",
                    InPath = true,
                    IsRequired = true,
                },
                new FunctionParams
                {
                    Name = "tenantId",
                    OriginalName = "x-tenant-id",
                    Type = "string",
                    InPath = false,
                    IsRequired = false,
                },
            ]
        };

        // Act
        var code = CSHttpClientGenerate.ToRequestFunction(function);

        // Assert
        Assert.Contains("GetAsync(string apiVersion, string? tenantId", code);
        Assert.Contains("var url = $\"/users/{apiVersion}?x-tenant-id={tenantId}\";", code);
    }

    [Fact]
    public async Task RealOpenApiJson_ShouldGenerateExpectedOutput_ForDotnetAndAngular()
    {
        // Arrange
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Generate", "Fixtures", "request-client-special.openapi.json");
        Assert.True(File.Exists(fixturePath), $"Fixture not found: {fixturePath}");

        var (doc, _) = await OpenApiDocument.LoadAsync(fixturePath);
        Assert.NotNull(doc);

        // Act - C#
        var csGen = new CSHttpClientGenerate(doc!);
        var csServiceFiles = csGen.GetServices("DemoClient");
        var csModelFiles = csGen.GetModelFiles("DemoClient");

        // Assert - C# service
        var csUserService = csServiceFiles.First(f => f.Name == "UserRestService.cs").Content;
        Assert.Contains("GetAsync(string apiVersion, string? xTenantId", csUserService);
        Assert.Contains("var url = $\"/users/{apiVersion}?x-tenant-id={xTenantId}\";", csUserService);

        // Assert - C# model
        var csSampleModel = csModelFiles.First(f => f.Name == "SampleDto.cs").Content;
        Assert.Contains("[JsonPropertyName(\"@id\")]", csSampleModel);
        Assert.Contains("[JsonPropertyName(\"#text\")]", csSampleModel);
        Assert.Contains("[JsonPropertyName(\"api-version\")]", csSampleModel);

        // Act - Angular
        var ngGen = new AngularClient(doc!);
        var ngServiceFiles = ngGen.GenerateServices(doc!.Tags!, "demo");
        var ngModelFiles = ngGen.GenerateModelFiles();

        // Assert - Angular service
        var ngUserService = ngServiceFiles.First(f => f.Name == "user.service.ts").Content;
        Assert.Contains("get(apiVersion: string, xTenantId: string | null): Observable<SampleDto>", ngUserService);
        Assert.Contains("const _url = `/users/${apiVersion}?x-tenant-id=${xTenantId ?? ''}`;", ngUserService);

        // Assert - Angular model
        var ngSampleModel = ngModelFiles.First(f => f.Name == "sample-dto.model.ts").Content;
        Assert.Contains("'@id': string;", ngSampleModel);
        Assert.Contains("'#text': string;", ngSampleModel);
        Assert.Contains("'api-version': string;", ngSampleModel);
    }

    [Fact]
    public async Task MultipartOpenApiJson_ShouldGenerateCSharpFileUploadClient()
    {
        // Arrange
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Generate", "Fixtures", "request-client-upload.openapi.json");
        Assert.True(File.Exists(fixturePath), $"Fixture not found: {fixturePath}");

        var (doc, _) = await OpenApiDocument.LoadAsync(fixturePath);
        Assert.NotNull(doc);

        // Act
        var generator = new CSHttpClientGenerate(doc!);
        var service = generator.GetServices("DemoClient").Single().Content;
        var models = generator.GetModelFiles("DemoClient");
        var baseService = CSHttpClientGenerate.GetBaseService("DemoClient");

        // Assert
        Assert.Contains(
            "LoadMapFileWaferMapLoadMapFilePostAsync(string mapId, Stream data, string fileName, CancellationToken cancellationToken = default)",
            service
        );
        Assert.Contains("var url = $\"/waferMap/loadMapFile?map_id={mapId}\";", service);
        Assert.Contains(
            "UploadFileAsync<object?>(url, new StreamContent(data), fileName, fieldName: \"map_file\", cancellationToken: cancellationToken)",
            service
        );
        Assert.Contains("string fileName = \"file\", string fieldName = \"file\"", baseService);
        Assert.Contains("{ file, fieldName, fileName }", baseService);

        var uploadModel = models.Single(file => file.Name == "BodyLoadMapFileWaferMapLoadMapFilePost.cs").Content;
        Assert.Contains("namespace DemoClient.Models;", uploadModel);
        Assert.DoesNotContain("namespace DemoClient.Models.;", uploadModel);
        Assert.Contains("public Stream MapFile", uploadModel);
    }
}
