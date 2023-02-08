using System.IO;
using System.Linq;
using Core.Infrastructure.Helper;
using Microsoft.OpenApi.Readers;

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

    [Fact]
    public void Should_parse_openApi()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Data\openapi.json");
        string openApiContent = File.ReadAllText(filePath);
        // 过滤特殊符号
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);
        var helper = new OpenApiHelper(apiDocument);

        var apis = helper.RestApiGroups;
        Assert.NotNull(helper.RestApiGroups);
    }

    [Fact]
    public void Should_parse_enum()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        var helper = new EntityParseHelper(filePath);
        helper.Parse();
        var enumSatus = helper.CompilationHelper.GetEnum("EnumType");

        Console.WriteLine();
    }
}
