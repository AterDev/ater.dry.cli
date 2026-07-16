using CoreMod.Services;
using Xunit;

namespace CoreMod.Tests.Services;

public class SolutionServiceAotTests : IDisposable
{
    private readonly string _testPath = Path.Combine(
        Path.GetTempPath(),
        "SolutionServiceAotTests_" + Guid.NewGuid()
    );

    public SolutionServiceAotTests()
    {
        Directory.CreateDirectory(_testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Theory]
    [InlineData("isAOT = true", true)]
    [InlineData("isAOT = false", false)]
    [InlineData("IsAot=true # AOT template", true)]
    public void IsAOT_ShouldReadRootPerigonConfig(string content, bool expected)
    {
        var configDirectory = Path.Combine(_testPath, ".config");
        Directory.CreateDirectory(configDirectory);
        File.WriteAllText(Path.Combine(configDirectory, "perigon.config.toml"), content);

        var result = SolutionService.IsAOT(_testPath);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsAOT_ShouldReturnFalse_WhenConfigIsMissing()
    {
        Assert.False(SolutionService.IsAOT(_testPath));
    }

    [Fact]
    public void IsAOT_ShouldIgnoreLegacySrcConfigPath()
    {
        var configDirectory = Path.Combine(_testPath, "src", ".config");
        Directory.CreateDirectory(configDirectory);
        File.WriteAllText(
            Path.Combine(configDirectory, "perigon.config.toml"),
            "isAOT = true"
        );

        Assert.False(SolutionService.IsAOT(_testPath));
    }
}
