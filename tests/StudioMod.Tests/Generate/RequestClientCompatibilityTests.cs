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

    [Theory]
    [InlineData("User Management", "UserManagement")]
    [InlineData("123 User", "Api123User")]
    public void NormalizeServiceName_ShouldProduceCodeIdentifier(string tagName, string expected)
    {
        Assert.Equal(expected, RequestClientHelper.NormalizeServiceName(tagName));
    }

    [Fact]
    public async Task RequestClients_ShouldNormalizeTagNamesForServicesAndClients()
    {
        // Arrange
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Generate", "Fixtures", "request-client-tag-spaces.openapi.json");
        var (doc, _) = await OpenApiDocument.LoadAsync(
            fixturePath,
            token: TestContext.Current.CancellationToken
        );
        Assert.NotNull(doc);

        // Act
        var csharpService = new CSHttpClientGenerate(doc!).GetServices("DemoClient").Single();
        var angularFiles = new AngularClient(doc!).GenerateServices(doc!.Tags!, "demo");
        var axiosService = new AxiosClient(doc!).GenerateServices(doc!.Tags!, "demo").Single();

        // Assert - C#
        Assert.Equal("UserManagementRestService.cs", csharpService.Name);
        Assert.Contains("public class UserManagementRestService", csharpService.Content);

        // Assert - Angular
        var angularService = angularFiles.Single(file => file.Name == "user-management.service.ts");
        var angularClient = angularFiles.Single(file => file.Name == "demo-client.ts");
        Assert.Contains("export class UserManagementService extends BaseService", angularService.Content);
        Assert.Contains("import { UserManagementService } from './services/user-management.service';", angularClient.Content);
        Assert.Contains("public userManagement = inject(UserManagementService);", angularClient.Content);

        // Assert - Axios
        Assert.Equal("user-management.service.ts", axiosService.Name);
        Assert.Contains("class UserManagementService extends BaseService", axiosService.Content);
    }

    [Fact]
    public void CSharpFormatter_ShouldEmitJsonPropertyName_WhenGeneratedPropertyNameDiffers()
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
                new PropertyInfo { Name = "snake_name", Type = "string" },
                new PropertyInfo { Name = "camelName", Type = "string" },
                new PropertyInfo { Name = "PascalName", Type = "string" },
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
        Assert.Contains("[JsonPropertyName(\"snake_name\")]", code);
        Assert.Contains("public string SnakeName { get; set; } = default!;", code);
        Assert.Contains("[JsonPropertyName(\"camelName\")]", code);
        Assert.Contains("public string CamelName { get; set; } = default!;", code);
        Assert.Contains("[JsonPropertyName(\"normalName\")]", code);
        Assert.Contains("public string NormalName { get; set; } = default!;", code);
        Assert.DoesNotContain("[JsonPropertyName(\"PascalName\")]", code);
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
        Assert.Contains("var url = $\"users/{apiVersion}?x-tenant-id={tenantId}\";", code);
    }

    [Fact]
    public void CSharpBaseService_ShouldUseRelativeRoutes_ForFileOperations()
    {
        // Arrange
        var baseService = CSHttpClientGenerate.GetBaseService("DemoClient");

        // Assert
        Assert.Contains("public ResponseContent? ResponseContent { get; set; }", baseService);
        Assert.Contains("Content = content", baseService);
        Assert.Contains("StatusCode = (int)response.StatusCode", baseService);
        Assert.Contains("ReasonPhrase = response.ReasonPhrase", baseService);
        Assert.DoesNotContain("ReadFromJsonAsync<ErrorResult>", baseService);
        Assert.Contains("UploadFileAsync<TResult>(HttpMethod method", baseService);
        Assert.Contains("SendMultipartAsync<TResult>(HttpMethod method", baseService);
        Assert.Contains("SendMultipartRequestAsync(method, route, content", baseService);
        Assert.Contains("new HttpRequestMessage(method, route)", baseService);
        Assert.Contains("Http.GetAsync(route, cancellationToken)", baseService);
        Assert.DoesNotContain("BuildRequestUrl", baseService);

        var client = CSHttpClientGenerate.GetClient([], "DemoClient", "Demo");
        Assert.Contains("public ResponseContent? ResponseContent", client);
        Assert.DoesNotContain("ErrorResult", client);
    }

    [Fact]
    public async Task RequestClients_ShouldGenerateNoContentAndMultipartHttpMethods()
    {
        // Arrange
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Generate", "Fixtures", "request-client-no-content.openapi.json");
        var (doc, _) = await OpenApiDocument.LoadAsync(
            fixturePath,
            token: TestContext.Current.CancellationToken
        );
        Assert.NotNull(doc);

        // Act
        var csharpService = new CSHttpClientGenerate(doc!).GetServices("DemoClient").Single().Content;
        var angularService = new AngularClient(doc!)
            .GenerateServices(doc!.Tags!, "demo")
            .Single(file => file.Name == "files.service.ts")
            .Content;
        var axiosService = new AxiosClient(doc!)
            .GenerateServices(doc!.Tags!, "demo")
            .Single()
            .Content;

        // Assert - C#
        Assert.Contains("public async Task DeleteFileAsync", csharpService);
        Assert.Contains("await SendNoContentAsync(HttpMethod.Delete, url, cancellationToken: cancellationToken);", csharpService);
        Assert.Contains("public async Task ReplaceFileAsync", csharpService);
        Assert.Contains("SendMultipartAsync(HttpMethod.Put, url, form, cancellationToken: cancellationToken)", csharpService);
        Assert.DoesNotContain("var url = $\"/", csharpService);
        Assert.DoesNotContain("Task<object?> DeleteFileAsync", csharpService);

        var putMultipartWithResponse = CSHttpClientGenerate.ToRequestFunction(new RequestServiceFunction
        {
            Name = "upload_file",
            Method = "Put",
            Path = "/files",
            ResponseType = "string",
            IsMultipart = true,
            Params =
            [
                new FunctionParams
                {
                    Name = "file",
                    OriginalName = "file",
                    InMultipart = true,
                    IsFile = true,
                    IsRequired = true,
                }
            ],
        });
        Assert.Contains("SendMultipartAsync<string?>(HttpMethod.Put, url, form", putMultipartWithResponse);

        // Assert - Angular
        Assert.Contains("deleteFile(fileId: string): Observable<void>", angularService);
        Assert.Contains("replaceFile(fileId: string, file: File): Observable<void>", angularService);
        Assert.Contains("return this.request<void>('put', _url, formData);", angularService);

        // Assert - Axios
        Assert.Contains("deleteFile(fileId: string, extOptions?: ExtOptions): Promise<void>", axiosService);
        Assert.Contains("replaceFile(fileId: string, file: File, extOptions?: ExtOptions): Promise<void>", axiosService);
        Assert.Contains("return this.request<void>('put', _url, formData, extOptions);", axiosService);

        var angularBase = RequestClientHelper.GetBaseService(RequestClientType.NgHttp);
        var axiosBase = RequestClientHelper.GetBaseService(RequestClientType.Axios);
        Assert.Contains("resp.status === 204", angularBase);
        Assert.Contains("response.status === 204", axiosBase);
        Assert.Contains("['post', 'put', 'patch'].includes(normalizedMethod)", axiosBase);
    }

    [Fact]
    public async Task RealOpenApiJson_ShouldGenerateExpectedOutput_ForDotnetAndAngular()
    {
        // Arrange
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Generate", "Fixtures", "request-client-special.openapi.json");
        Assert.True(File.Exists(fixturePath), $"Fixture not found: {fixturePath}");

        var (doc, _) = await OpenApiDocument.LoadAsync(
            fixturePath,
            token: TestContext.Current.CancellationToken
        );
        Assert.NotNull(doc);

        // Act - C#
        var csGen = new CSHttpClientGenerate(doc!);
        var csServiceFiles = csGen.GetServices("DemoClient");
        var csModelFiles = csGen.GetModelFiles("DemoClient");

        // Assert - C# service
        var csUserService = csServiceFiles.First(f => f.Name == "UserRestService.cs").Content;
        Assert.Contains("GetAsync(string apiVersion, string? xTenantId", csUserService);
        Assert.Contains("var url = $\"users/{apiVersion}?x-tenant-id={xTenantId}\";", csUserService);

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

        var (doc, _) = await OpenApiDocument.LoadAsync(
            fixturePath,
            token: TestContext.Current.CancellationToken
        );
        Assert.NotNull(doc);

        // Act
        var generator = new CSHttpClientGenerate(doc!);
        var service = generator.GetServices("DemoClient").Single().Content;
        var models = generator.GetModelFiles("DemoClient");
        var baseService = CSHttpClientGenerate.GetBaseService("DemoClient");

        // Assert
        Assert.Contains(
            "LoadMapFileWaferMapLoadMapFilePostAsync(string mapId, MultipartFile mapFile, CancellationToken cancellationToken = default)",
            service
        );
        Assert.Contains("var url = $\"waferMap/loadMapFile?map_id={mapId}\";", service);
        Assert.Contains(
            "form.Add(CreateMultipartFileContent(mapFile), \"map_file\", mapFile.FileName);",
            service
        );
        Assert.Contains("string fileName = \"file\", string fieldName = \"file\"", baseService);
        Assert.Contains("{ file, fieldName, fileName }", baseService);

        var uploadModel = models.Single(file => file.Name == "BodyLoadMapFileWaferMapLoadMapFilePost.cs").Content;
        Assert.Contains("namespace DemoClient.Models;", uploadModel);
        Assert.DoesNotContain("namespace DemoClient.Models.;", uploadModel);
        Assert.Contains("public Stream MapFile", uploadModel);
    }

    [Fact]
    public async Task MultipartOpenApi31Json_ShouldGenerateCSharpFileUploadClient()
    {
        // Arrange
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Generate", "Fixtures", "request-client-upload.openapi31.json");
        Assert.True(File.Exists(fixturePath), $"Fixture not found: {fixturePath}");

        var (doc, _) = await OpenApiDocument.LoadAsync(
            fixturePath,
            token: TestContext.Current.CancellationToken
        );
        Assert.NotNull(doc);

        // Act
        var generator = new CSHttpClientGenerate(doc!);
        var service = generator.GetServices("DemoClient").Single().Content;
        var models = generator.GetModelFiles("DemoClient");

        // Assert
        Assert.Contains("MultipartFile mapFile", service);
        Assert.Contains("IEnumerable<MultipartFile>? attachments", service);
        Assert.Contains("string? description", service);
        Assert.Contains("bool? overwrite", service);
        Assert.Contains(
            "form.Add(CreateMultipartFileContent(mapFile), \"map_file\", mapFile.FileName);",
            service
        );

        var uploadModel = models.Single(file => file.Name == "BodyLoadMapFileWaferMapLoadMapFilePost.cs").Content;
        Assert.Contains("public Stream MapFile", uploadModel);

        // Angular
        var angularService = new AngularClient(doc!)
            .GenerateServices(doc!.Tags!, "demo")
            .Single(file => file.Name == "wafer-map.service.ts")
            .Content;
        Assert.Contains("mapId: string, mapFile: File", angularService);
        Assert.Contains("attachments: File[] | null", angularService);
        Assert.Contains("description: string | null", angularService);
        Assert.Contains("overwrite: boolean | null", angularService);
        Assert.Contains("formData.append('map_file', mapFile, mapFile.name);", angularService);
        Assert.Contains("attachments.forEach(file => formData.append('attachments', file, file.name));", angularService);
        Assert.Contains("formData.append('description', String(description));", angularService);

        // Axios
        var axiosService = new AxiosClient(doc!).GenerateServices(doc!.Tags!, "demo").Single().Content;
        Assert.Contains("mapId: string, mapFile: File", axiosService);
        Assert.Contains("attachments: File[] | null", axiosService);
        Assert.Contains("description: string | null", axiosService);
        Assert.Contains("overwrite: boolean | null", axiosService);
        Assert.Contains("formData.append('map_file', mapFile, mapFile.name);", axiosService);
        Assert.Contains("attachments.forEach(file => formData.append('attachments', file, file.name));", axiosService);
    }
}
