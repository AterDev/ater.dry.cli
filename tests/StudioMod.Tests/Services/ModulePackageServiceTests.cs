using CoreMod.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Share;
using Share.Models;
using System.IO.Compression;
using System.Text.Json;
using Xunit;

namespace CoreMod.Tests.Services;

public class ModulePackageServiceTests
{
    [Fact]
    public async Task PackageModuleAsync_ShouldCreatePackage_ForSimpleAttributes()
    {
        var metadata = await RunPackageAsync(
            """
            namespace CMSMod;

            [DisplayName("Perigon::CMSMod")]
            [Description("包含内容管理相关功能")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }

                public static WebApplication UseCMSModServices(this WebApplication app)
                {
                    return app;
                }
            }
            """
        );

        Assert.NotNull(metadata);
        Assert.Equal("CMSMod", metadata!.ModuleName);
        Assert.Equal("Perigon", metadata.Author);
        Assert.Equal("CMSMod", metadata.DisplayName);
        Assert.Equal("包含内容管理相关功能", metadata.Description);
        Assert.True(metadata.UseSelfServices);
    }

    [Fact]
    public async Task PackageModuleAsync_ShouldCreatePackage_ForQualifiedAttributes()
    {
        var metadata = await RunPackageAsync(
            """
            namespace CMSMod;

            [global::System.ComponentModel.DisplayNameAttribute("Perigon::CMSMod")]
            [global::System.ComponentModel.DescriptionAttribute("包含内容管理相关功能")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }

                public static WebApplication UseCMSModServices(this WebApplication app)
                {
                    return app;
                }
            }
            """
        );

        Assert.NotNull(metadata);
        Assert.Equal("Perigon", metadata!.Author);
        Assert.Equal("CMSMod", metadata.DisplayName);
        Assert.Equal("包含内容管理相关功能", metadata.Description);
    }

    private static async Task<PackageMetadata?> RunPackageAsync(string moduleExtensionsContent)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var modulesPath = Path.Combine(root, PathConst.ModulesPath);
        var entityPath = Path.Combine(root, PathConst.EntityPath);
        var servicesPath = Path.Combine(root, PathConst.ServicesPath);

        Directory.CreateDirectory(Path.Combine(modulesPath, "CMSMod"));
        Directory.CreateDirectory(Path.Combine(entityPath, "CMSMod"));
        Directory.CreateDirectory(Path.Combine(servicesPath, "AdminService"));

        await File.WriteAllTextAsync(
            Path.Combine(modulesPath, "CMSMod", ConstVal.ModuleExtensionFile),
            moduleExtensionsContent,
            TestContext.Current.CancellationToken
        );

        await File.WriteAllTextAsync(
            Path.Combine(entityPath, "CMSMod", "ContentEntity.cs"),
            "public class ContentEntity { }",
            TestContext.Current.CancellationToken
        );

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var solutionContext = new SolutionContext(serviceProvider)
        {
            SolutionPath = root,
            ModulesPath = modulesPath,
            EntityPath = entityPath,
            ServicesPath = servicesPath
        };

        var localizerMock = new Mock<IStringLocalizer<Localizer>>();
        localizerMock
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        localizerMock
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var service = new ModulePackageService(
            solutionContext,
            new Localizer(localizerMock.Object),
            NullLogger<ModulePackageService>.Instance
        );

        try
        {
            var packagePath = await service.PackageModuleAsync("CMSMod", "AdminService");
            Assert.NotNull(packagePath);
            Assert.True(File.Exists(packagePath));

            using var archive = ZipFile.OpenRead(packagePath!);
            var metadataEntry = archive.GetEntry("metadata.json");
            Assert.NotNull(metadataEntry);

            using var reader = new StreamReader(metadataEntry!.Open());
            var metadataJson = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            return JsonSerializer.Deserialize<PackageMetadata>(
                metadataJson,
                ConstVal.DefaultJsonSerializerOptions
            );
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}