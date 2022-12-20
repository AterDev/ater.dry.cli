using Core.Infrastructure.Helper;

namespace CodeGenerator.Test;
public class FunctionTest
{
    [Fact]
    public void Should_parse_entity_attribute()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        var helper = new EntityParseHelper(filePath);
        helper.Parse();
        Console.WriteLine();
    }

    [Fact]

    public void TestString()
    {
        string? a = string.Empty;
        string? b = string.Empty;


        var c = a == b;

        Assert.True(c);

    }
}
