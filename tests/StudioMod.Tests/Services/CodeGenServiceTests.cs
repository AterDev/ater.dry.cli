using CodeGenerator;
using CoreMod.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Share;
using Share.Models;
using Share.Services;
using Xunit;

namespace CoreMod.Tests.Services;

public class CodeGenServiceTests
{
    [Fact]
    public async Task GenerateControllerAsync_ShouldUseDefaultConfig_WhenSolutionConfigIsMissing()
    {
        var servicePath = Path.Combine(Path.GetTempPath(), $"CodeGenServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(servicePath);

        try
        {
            var serviceName = Path.GetFileName(servicePath);
            await File.WriteAllTextAsync(
                Path.Combine(servicePath, $"{serviceName}.csproj"),
                "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>",
                TestContext.Current.CancellationToken
            );

            var projectContext = new SolutionContext(new ServiceCollection().BuildServiceProvider());
            var cache = new CacheService(new MemoryCache(new MemoryCacheOptions()));
            var codeGenService = new CodeGenService(
                NullLogger<CodeGenService>.Instance,
                projectContext,
                cache
            );
            var entityInfo = new EntityInfo
            {
                Name = "Product",
                NamespaceName = "Store.Domain.Entities",
                ModuleName = "StoreMod",
                FilePath = "Product.cs",
            };

            var files = await codeGenService.GenerateControllerAsync(
                entityInfo,
                servicePath,
                "namespace @Model.Namespace; public class @(Model.EntityName)Controller {}"
            );

            Assert.NotNull(projectContext.SolutionConfig);
            Assert.Equal("SystemMod", projectContext.SolutionConfig.SystemModName);
            Assert.Contains(files, file => file.Name == "ProductController.cs");
            Assert.Contains(files, file => file.Name == ConstVal.GlobalUsingsFile);
        }
        finally
        {
            if (Directory.Exists(servicePath))
            {
                Directory.Delete(servicePath, true);
            }
        }
    }
}
