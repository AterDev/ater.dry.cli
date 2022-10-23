using CodeGenerator.Generate;
using CodeGenerator.Infrastructure.Helper;
using CodeGenerator.Test.Entity;
using System.IO;
using System.Linq;

namespace CodeGenerator.Test;

public class DtoGenerateTest
{
    [Fact]
    public void Should_find_project_file()
    {
        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        var root = dir.Root;
        var projectFile = AssemblyHelper.FindProjectFile(dir, root);
        var name = AssemblyHelper.GetAssemblyName(projectFile!);

        Assert.Equal("CodeGenerator.Test.csproj", projectFile!.Name);
        Assert.Equal("CodeGenerator.Test", name);
    }

    [Fact]
    public void Shoud_parse_entity_and_properties()
    {
        var filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        //filePath = @"C:\self\DevPlatform\src\Core\Identity\Account.cs";
        var entityHelper = new EntityParseHelper(filePath);
        entityHelper.Parse();
        Assert.Equal("Blog", entityHelper.Name);
        var assembly = typeof(Blog).Assembly.ManifestModule.Name;
        Assert.Equal(assembly, entityHelper.AssemblyName + ".dll");

        var props = entityHelper.PropertyInfos!;
        Assert.NotEmpty(props);

        var idProp = props!.Where(p => p.Name.Equals("Id")).FirstOrDefault();
        Assert.NotNull(idProp);
        Assert.False(idProp!.IsNullable);

        // 验证属性内容
        var nameProp = props!.Where(p => p.Name.Equals("Name")).FirstOrDefault();
        Assert.True(nameProp!.IsRequired);
        Assert.False(nameProp!.IsNullable);
        Assert.Equal(100, nameProp!.MaxLength);
        Assert.Equal(10, nameProp!.MinLength);

        var titleProp = props!.Where(p => p.Name.Equals("Title")).FirstOrDefault();
        Assert.False(titleProp!.IsNullable);

        var commentsProp = props!.Where(p => p.Name.Equals("Comments")).FirstOrDefault();
        Assert.True(commentsProp!.IsList);
        Assert.True(commentsProp!.IsNullable);
        Assert.True(commentsProp!.IsNavigation);
        Assert.Equal("Comments", commentsProp!.NavigationName);

        var commentProp = props!.SingleOrDefault(p => p.Name.Equals("Comments2"));
        Assert.True(commentsProp!.IsNavigation);
        Assert.True(commentsProp!.IsNullable);

        var statusProp = props!.Where(p => p.Name.Equals("Status")).FirstOrDefault();
        Assert.True(statusProp!.IsEnum);
        Assert.False(statusProp!.IsNavigation);

        // 父类属性
        var datetimeProp = props!.SingleOrDefault(p => p.Name.Equals("CreatedTime"));
        Assert.Equal("DateTimeOffset", datetimeProp!.Type);
    }


    [Fact]
    public void Shoud_generate_dto_content()
    {
        var filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        //var filePath = PathHelper.GetProjectFilePath(@"D:\codes\DevPlatform\src\Microservice\DocAPI\Models\DocsCatalog.cs");
        var dtoPath = PathHelper.GetProjectPath();
        var gen = new DtoCodeGenerate(filePath,dtoPath);
        var shortDto = gen.GetShortDto();
        var filterDto = gen.GetFilterDto();
        var updateDto = gen.GetUpdateDto();
        var itemDto = gen.GetItemDto();
        Assert.NotNull(shortDto);
        Assert.NotNull(filterDto);
        Assert.NotNull(updateDto);
        Assert.NotNull(itemDto);
    }
}
