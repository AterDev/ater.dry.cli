using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using CodeGenerator.Generate;
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
        var props = helper.PropertyInfos;
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
        var members = helper.GetEnumMembers("EnumType");
        Assert.NotNull(members);
        var condition = members.Any(m => m!.Name.Equals("Default"));
        Assert.True(condition);
    }

    [Fact]
    public void Should_generate_protobuf()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        var gen = new ProtobufGenerate(filePath);
        var proto = gen.GenerateProtobuf();
        Console.WriteLine(proto);
    }

    [Theory]
    [InlineData("IList<abc>")]
    [InlineData("List<abc>")]
    [InlineData("ICollection<abc>")]
    [InlineData("IEnumerable<abc>")]
    public void Should_regex_listType(string type)
    {
        var originType = EntityParseHelper.GetTypeFromList(type);
        Assert.Equal("abc", originType);
    }

    [Fact]
    public void Should_get_projectType()
    {
        var current = PathHelper.GetProjectPath();

        var projectFile = Path.Combine(current, "CodeGenerator.Test.csproj");

        var type = AssemblyHelper.GetProjectType(new FileInfo(projectFile));
        Assert.Equal("console", type);
    }

    [Fact]
    public void Should_Parse_Interface()
    {
        var projectPath = @"C:\codes\ater.web\templates\apistd\src\Application\";
        var interfaceName = "ISystemConfigManager";
        var filePath = Path.Combine(projectPath, "IManager", interfaceName + ".cs");

        var compilation = new CompilationHelper(projectPath);
        compilation.AddSyntaxTree(File.ReadAllText(filePath));

        var exist = compilation.MethodExist("Task<SystemConfig?> GetOwnedAsync(Guid id);");
        Assert.True(exist);

        compilation.InsertInterfaceMethod("Task<SystemConfig?> GetOwnedAsync(int id);");
        var content = compilation.SyntaxRoot!.ToFullString();

        Console.WriteLine();

    }

    [Fact]
    public void Test_JsonUpdate()
    {
        var jsonString = """
                        {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft": "Warning",
                  "Microsoft.Hosting.Lifetime": "Information"
                }
              },
              "AllowedHosts": "*"
            }
            
            """;
        var jsonNode = JsonNode.Parse(jsonString);
        JsonHelper.UpdateJsonNode(jsonNode, "AllowedHosts", "111");

        Console.WriteLine();

    }
}
