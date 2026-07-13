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
        var result = await RunPackageAsync(
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

        Assert.NotNull(result.Metadata);
        Assert.Equal("CMSMod", result.Metadata!.ModuleName);
        Assert.Equal("Perigon", result.Metadata.Author);
        Assert.Equal("CMSMod", result.Metadata.DisplayName);
        Assert.Equal("包含内容管理相关功能", result.Metadata.Description);
        Assert.True(result.Metadata.UseSelfServices);
        Assert.Null(result.Metadata.Frontend);
    }

    [Fact]
    public async Task PackageModuleAsync_ShouldCreatePackage_ForQualifiedAttributes()
    {
        var result = await RunPackageAsync(
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

        Assert.NotNull(result.Metadata);
        Assert.Equal("Perigon", result.Metadata!.Author);
        Assert.Equal("CMSMod", result.Metadata.DisplayName);
        Assert.Equal("包含内容管理相关功能", result.Metadata.Description);
    }

    [Fact]
    public async Task PackageModuleAsync_ShouldIncludeAngularFrontend_WhenFrontPathIsSpecified()
    {
        var result = await RunPackageAsync(
            """
            namespace CMSMod;

            [DisplayName("Perigon::CMSMod")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }
            }
            """,
            frontendPackageJson: """{"dependencies":{"@angular/core":"20.0.0"}}"""
        );

        Assert.NotNull(result.Metadata);
        Assert.Equal("Angular", result.Metadata!.Frontend);
        Assert.Contains("Frontend/cms.component.ts", result.EntryNames);
    }

    [Theory]
    [InlineData("""{"dependencies":{"react":"19.0.0"}}""", "React")]
    [InlineData("""{"devDependencies":{"vue":"3.0.0"}}""", "Vue")]
    [InlineData("""{"dependencies":{"svelte":"5.0.0"}}""", "Else")]
    public async Task PackageModuleAsync_ShouldDetectFrontendType(string packageJson, string expectedFrontend)
    {
        var result = await RunPackageAsync(
            """
            namespace CMSMod;

            [DisplayName("Perigon::CMSMod")]
            public static class ModuleExtensions
            {
                public static IHostApplicationBuilder AddCMSMod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }
            }
            """,
            frontendPackageJson: packageJson
        );

        Assert.Equal(expectedFrontend, result.Metadata!.Frontend);
    }

    private static async Task<PackageResult> RunPackageAsync(
        string moduleExtensionsContent,
        string? frontendPackageJson = null
    )
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

        string? frontendPath = null;
        if (frontendPackageJson != null)
        {
            frontendPath = Path.Combine(root, "frontend", "features", "cms");
            Directory.CreateDirectory(frontendPath);
            await File.WriteAllTextAsync(
                Path.Combine(frontendPath, "cms.component.ts"),
                "export class CmsComponent {}",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(root, "frontend", ConstVal.NodeProjectFile),
                frontendPackageJson,
                TestContext.Current.CancellationToken
            );
        }

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
            var packagePath = await service.PackageModuleAsync("CMSMod", "AdminService", frontendPath);
            Assert.NotNull(packagePath);
            Assert.True(File.Exists(packagePath));

            using var archive = ZipFile.OpenRead(packagePath!);
            var metadataEntry = archive.GetEntry("metadata.json");
            Assert.NotNull(metadataEntry);

            using var reader = new StreamReader(metadataEntry!.Open());
            var metadataJson = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            var metadata = JsonSerializer.Deserialize<PackageMetadata>(
                metadataJson,
                ConstVal.DefaultJsonSerializerOptions
            );
            var entryNames = archive.Entries.Select(entry => entry.FullName).ToList();
            return new PackageResult(metadata, entryNames);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    private sealed record PackageResult(PackageMetadata? Metadata, List<string> EntryNames);
}
