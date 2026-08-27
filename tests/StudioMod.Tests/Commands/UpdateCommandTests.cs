using CommandLine.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Share;
using Xunit;

namespace CoreMod.Tests.Commands;

[CollectionDefinition("CurrentDirectory", DisableParallelization = true)]
public sealed class CurrentDirectoryCollection
{
}

[Collection("CurrentDirectory")]
public sealed class UpdateCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldFailOutsideASolutionDirectory()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"perigon-update-command-{Guid.NewGuid():N}"
        );
        Directory.CreateDirectory(temporaryDirectory);
        var originalDirectory = Environment.CurrentDirectory;

        try
        {
            Environment.CurrentDirectory = temporaryDirectory;
            var projectContext = new SolutionContext(
                new ServiceCollection().BuildServiceProvider()
            );
            var command = new UpdateCommand(
                null!,
                projectContext,
                CreateLocalizer()
            );

            var exitCode = await command.ExecuteAsync(null!, cancellationToken);

            Assert.Equal(1, exitCode);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    private static Localizer CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<Localizer>>();
        localizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] _) => new LocalizedString(name, name));
        return new Localizer(localizer.Object);
    }
}
