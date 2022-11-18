using System.IO;
using System.Linq;
using CodeGenerator.Generate;
using CodeGenerator.Test.Entity;
using Core.Infrastructure.Helper;

namespace CodeGenerator.Test;

public class DtoGenerateTest
{
    [Fact]
    public void Should_find_project_file()
    {
        DirectoryInfo dir = new(Environment.CurrentDirectory);
        DirectoryInfo root = dir.Root;
        FileInfo? projectFile = AssemblyHelper.FindProjectFile(dir, root);
        string name = AssemblyHelper.GetAssemblyName(projectFile!);

        Assert.Equal("CodeGenerator.Test.csproj", projectFile!.Name);
        Assert.Equal("CodeGenerator.Test", name);
    }

    [Fact]
    public void Shoud_parse_entity_and_properties()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        //filePath = @"C:\self\DevPlatform\src\Core\Identity\Account.cs";
        EntityParseHelper entityHelper = new(filePath);
        entityHelper.Parse();
        Assert.Equal("Blog", entityHelper.Name);
        string assembly = typeof(Blog).Assembly.ManifestModule.Name;
        Assert.Equal(assembly, entityHelper.AssemblyName + ".dll");

        List<Core.Models.PropertyInfo> props = entityHelper.PropertyInfos!;
        Assert.NotEmpty(props);

        Core.Models.PropertyInfo? idProp = props!.Where(p => p.Name.Equals("Id")).FirstOrDefault();
        Assert.NotNull(idProp);
        Assert.False(idProp!.IsNullable);

        // 验证属性内容
        Core.Models.PropertyInfo? nameProp = props!.Where(p => p.Name.Equals("Name")).FirstOrDefault();
        Assert.True(nameProp!.IsRequired);
        Assert.False(nameProp!.IsNullable);
        Assert.Equal(100, nameProp!.MaxLength);
        Assert.Equal(10, nameProp!.MinLength);

        Core.Models.PropertyInfo? titleProp = props!.Where(p => p.Name.Equals("Title")).FirstOrDefault();
        Assert.False(titleProp!.IsNullable);

        Core.Models.PropertyInfo? commentsProp = props!.Where(p => p.Name.Equals("Comments")).FirstOrDefault();
        Assert.True(commentsProp!.IsList);
        Assert.True(commentsProp!.IsNullable);
        Assert.True(commentsProp!.IsNavigation);
        Assert.Equal("Comments", commentsProp!.NavigationName);

        Core.Models.PropertyInfo? commentProp = props!.SingleOrDefault(p => p.Name.Equals("Comments2"));
        Assert.True(commentsProp!.IsNavigation);
        Assert.True(commentsProp!.IsNullable);

        Core.Models.PropertyInfo? statusProp = props!.Where(p => p.Name.Equals("Status")).FirstOrDefault();
        Assert.True(statusProp!.IsEnum);
        Assert.False(statusProp!.IsNavigation);

        // 父类属性
        Core.Models.PropertyInfo? datetimeProp = props!.SingleOrDefault(p => p.Name.Equals("CreatedTime"));
        Assert.Equal("DateTimeOffset", datetimeProp!.Type);
    }


    [Fact]
    public void Shoud_generate_dto_content()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        //var filePath = PathHelper.GetProjectFilePath(@"D:\codes\DevPlatform\src\Microservice\DocAPI\Models\DocsCatalog.cs");
        string dtoPath = PathHelper.GetProjectPath();
        DtoCodeGenerate gen = new(filePath, dtoPath);
        string? shortDto = gen.GetShortDto();
        string? filterDto = gen.GetFilterDto();
        string? updateDto = gen.GetUpdateDto();
        string? itemDto = gen.GetItemDto();
        Assert.NotNull(shortDto);
        Assert.NotNull(filterDto);
        Assert.NotNull(updateDto);
        Assert.NotNull(itemDto);
    }
}
