using System.IO;
using System.Linq;

using CodeGenerator.Generate;
using CodeGenerator.Test.Entity;

using Definition.Entity;
using Definition.EntityFramework;
using Definition.Infrastructure.Helper;

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
        //filePath = @"D:\codes\ater.web\templates\apistd\src\Entity\SystemEntities\SystemPermission.cs";
        EntityParseHelper entityHelper = new(filePath);
        entityHelper.Parse();

        Assert.Equal("Blog", entityHelper.Name);
        string assembly = typeof(Blog).Assembly.ManifestModule.Name;
        Assert.Equal(assembly, entityHelper.AssemblyName + ".dll");

        List<PropertyInfo> props = entityHelper.PropertyInfos!;
        Assert.NotEmpty(props);

        PropertyInfo? idProp = props!.Where(p => p.Name.Equals("Id")).FirstOrDefault();
        Assert.NotNull(idProp);
        Assert.False(idProp!.IsNullable);

        // 验证属性内容
        PropertyInfo? nameProp = props!.Where(p => p.Name.Equals("Name")).FirstOrDefault();
        Assert.True(nameProp!.IsRequired);
        Assert.False(nameProp!.IsNullable);
        Assert.Equal(100, nameProp!.MaxLength);
        Assert.Equal(10, nameProp!.MinLength);

        PropertyInfo? titleProp = props!.Where(p => p.Name.Equals("Title")).FirstOrDefault();
        Assert.False(titleProp!.IsNullable);

        PropertyInfo? commentsProp = props!.Where(p => p.Name.Equals("Comments")).FirstOrDefault();
        Assert.True(commentsProp!.IsList);
        Assert.True(commentsProp!.IsNullable);
        Assert.True(commentsProp!.IsNavigation);
        Assert.Equal("Comments", commentsProp!.NavigationName);

        PropertyInfo? commentProp = props!.SingleOrDefault(p => p.Name.Equals("Comments2"));
        Assert.True(commentsProp!.IsNavigation);
        Assert.True(commentsProp!.IsNullable);

        PropertyInfo? statusProp = props!.Where(p => p.Name.Equals("Status")).FirstOrDefault();
        Assert.True(statusProp!.IsEnum);
        Assert.False(statusProp!.IsNavigation);

        // 父类属性
        PropertyInfo? datetimeProp = props!.SingleOrDefault(p => p.Name.Equals("CreatedTime"));
        Assert.Equal("DateTimeOffset", datetimeProp!.Type);
    }


    [Fact]
    public void Shoud_generate_dto_content()
    {
        string filePath = PathHelper.GetProjectFilePath(@"Entity\Blog.cs");
        filePath = @"D:\codes\ater.web\templates\apistd\src\Entity\SystemEntities\SystemPermission.cs";
        string dtoPath = PathHelper.GetProjectPath();
        DtoCodeGenerate gen = new(filePath, dtoPath, new DryContext());
        string? addDto = gen.GetAddDto();
        string? shortDto = gen.GetShortDto();
        string? filterDto = gen.GetFilterDto();
        string? updateDto = gen.GetUpdateDto();
        string? itemDto = gen.GetItemDto();
        Assert.NotNull(addDto);
        Assert.NotNull(shortDto);
        Assert.NotNull(filterDto);
        Assert.NotNull(updateDto);
        Assert.NotNull(itemDto);
    }
}
