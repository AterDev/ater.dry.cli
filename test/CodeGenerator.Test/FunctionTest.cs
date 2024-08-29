using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using CodeGenerator.Generate;

using Definition.Infrastructure.Helper;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Readers;

using NuGet.Versioning;
using RazorEngineCore;

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
        compilation.LoadContent(File.ReadAllText(filePath));

        var exist = compilation.MethodExist("Task<SystemConfig?> GetOwnedAsync(Guid id);");
        Assert.True(exist);

        compilation.InsertInterfaceMethod("Task<SystemConfig?> GetOwnedAsync(int id);");
        var content = compilation.SyntaxRoot!.ToFullString();

        Console.WriteLine();

    }

    [Fact]
    public void Test_JsonNode()
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
                // 应用组件配置
                "Components": {
                  // 数据支持: pgsql/sqlserver
                  "Database": "postgresql",
                  // 缓存支持: redis/memory/none
                  "Cache": "redis",
                  // 日志支持: otlp/none
                  "Logging": "none",
                  // 是否使用swagger
                  "Swagger": true,
                  // 是否使用 jwt 验证
                  "Jwt": true
                },
                "ConnectionStrings": {
                  "Default": "Server=localhost;Port=5432;Database=v7._0;User Id=postgres;Password=root;",
                  "Redis": "localhost:6379",
                  "RedisInstanceName": "Dev"
                },
              "AllowedHosts": "*"
            }
            
            """;
        var jsonNode = JsonNode.Parse(jsonString, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip
        });
        var section = JsonHelper.GetSectionNode(jsonNode!, "ConnectionStrings");
        var value = JsonHelper.GetValue<string>(section!, "RedisInstanceName");
        JsonHelper.AddOrUpdateJsonNode(jsonNode!, "AllowedHosts", "111");
        var changeValaue = JsonHelper.GetValue<string>(jsonNode!, "AllowedHosts");
        Assert.Equal("Dev", value);
        Assert.Equal("111", changeValaue);
    }

    [Fact]
    public void Should_Version()
    {
        var v70 = NuGetVersion.Parse("7.0");
        var v700 = NuGetVersion.Parse("7.0.0");
        var v71 = NuGetVersion.Parse("7.1");
        var equal = VersionComparer.Compare(v70, v700, VersionComparison.Version) == 0;

        Assert.True(equal);

    }

    [Fact]
    public async Task Should_Analysis_CodeAsync()
    {
        var content = File.ReadAllText(@"D:\codes\v7.0\src\Application\IManager\ISystemRoleManager.cs");
        var tree = CSharpSyntaxTree.ParseText(content);
        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { mscorlib });
        await CSharpAnalysisHelper.GetBaseInterfaceInfoAsync(compilation, tree);

    }

    [Fact]
    public void Should_ConvertJsonToCsharp()
    {
        var json = """
            {
              "description": {
                "title": "Contiguous U.S., Average Temperature",
                "units": "Degrees Fahrenheit",
                "base_period": "1901-2000",
                "time":"12:00:12",
                "date":"2013-12-12",
                "datetime":"2022-12-12T12:22:22Z"
              },
              "list":[
                  {
                      "text":"abc",
                      "des":"des",
                      "detail":null
                  }
              ],
              "enum":[1,2,3,4],
              "summary": {
                "title": "Contiguous U.S., Average Temperature",
                "units": "Degrees Fahrenheit",
                "base_period": "1901-2000",
                "abc":"222"
              },
              "data": {
                "189512": {
                  "value": "50.34",
                  "anomaly": "-1.68"
                },
                "189612": {
                  "value": "51.99",
                  "anomaly": "-0.03"
                },
                "188712": {
                  "value": "51.56",
                  "anomaly": "-0.46"
                },
                "adf":{
                    "aaa":"1111"
                }
              }
            }
            """;

        var helper = new CSharpCovertHelper();
        if (CSharpCovertHelper.CheckJson(json))
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            helper.ToCsharpClassContent(jsonElement);

            var meta = helper.JsonMetadataList;
            helper.GenerateClass(jsonElement);


            var classcodes = helper.ClassCodes;
            Console.WriteLine(classcodes.Count);

        }
    }

    [Fact]
    public void GetBaseLists_Should_Return_BaseTypes()
    {
        // Arrange
        var compilationHelper = new CompilationHelper("./");
        compilationHelper.LoadContent("public class MyClass : MyBaseClass, IEntityBase { }");

        // Act
        var baseTypes = compilationHelper.GetBaseLists();

        // Assert
        Assert.NotNull(baseTypes);

        Assert.Equal("MyBaseClass", baseTypes[0].ToString());

        baseTypes = compilationHelper.GetBaseLists("IEntityBase");
        Assert.NotNull(baseTypes);
        Assert.Equal("IEntityBase", baseTypes[0].ToString());
    }


    [Fact]
    public void ShouldParseRazorEngine()
    {
        var templateStr = """
            using @(Model.Namespace).Manager;
            using @(Model.EntityName??"123")Manager;
            """;

        IRazorEngine razorEngine = new RazorEngine();
        var template = razorEngine.Compile<RazorEngineTemplateBase<Model>>(templateStr);

        string result = template.Run(instance =>
        {
            instance.Model = new Model
            {
                Namespace = "System",
                //EntityName = "User"
            };
        });
        Console.WriteLine(result);
    }
}

public class Model
{
    public string Namespace { get; set; } = "";
    public string? EntityName { get; set; }
}
