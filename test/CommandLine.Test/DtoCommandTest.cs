
using CodeGenerator.Test.Hepler;
using Xunit;

namespace CommandLine.Test;

public class DtoCommandTest
{
    [Fact]
    public void Shoud_generate_files()
    {
        var projectPath = PathHelper.GetProjectPath();
        var entityFilePath =  PathHelper.GetProjectFilePath(@"..\CodeGenerator.Test\Entity\Blog.cs");
        var outputPath = projectPath;
        var cmd = new DtoCommand(entityFilePath, outputPath);
        cmd.Run();

        var entityName = Path.GetFileNameWithoutExtension(new FileInfo(entityFilePath).Name);
        var generateFile = Path.Combine(outputPath, "Models", $"{entityName}Dtos", $"{entityName}UpdateDto.cs");
        var usingFile = Path.Combine(outputPath, "Models", $"GlobalUsing.cs");
        Assert.True(File.Exists(generateFile));
        Assert.True(File.Exists(usingFile));
    }
}
