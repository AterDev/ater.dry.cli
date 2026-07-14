using CodeGenerator;
using Xunit;

namespace CoreMod.Tests.Generate;

public class TplContentTests
{
    [Fact]
    public void ModuleExtension_ShouldNotGenerateUseModuleServicesMethod()
    {
        var content = TplContent.ModuleExtension("SampleMod");

        Assert.Contains("AddSampleMod", content);
        Assert.DoesNotContain("UseSampleModServices", content);
    }
}
