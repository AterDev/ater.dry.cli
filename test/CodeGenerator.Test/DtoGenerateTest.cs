using CodeGenerator.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGenerator.Test;

public class DtoGenerateTest
{
    [Fact]
    public void Should_find_project_file()
    {
        var dir = new DirectoryInfo(Environment.CurrentDirectory);
        var root = dir.Root;
        var projectFile = AssemblyHelper.FindProjectFile(dir, root);
        var name = AssemblyHelper.GetAssemblyName(projectFile);

        Assert.Equal("CodeGenerator.Test.csproj", projectFile.Name);
        Assert.Equal("CodeGenerator.Test", name);
    }

    [Fact]
    public void Shoud_parse_entity_properties()
    {
        var filePath = @"C:\self\cli\test\CodeGenerator.Test\Entity\Blog.cs";
        var entityHelper = new EntityParseHelper(filePath);

        var props = entityHelper.GetPropertyInfos("Blog");
        Assert.NotEmpty(props);

        // 验证属性内容
        var nameProp = props!.Where(p => p.Name.Equals("Name")).FirstOrDefault();
        Assert.True(nameProp!.IsRequired);
        Assert.True(nameProp!.IsNullable);
        Assert.Equal(100, nameProp!.MaxLength);
        Assert.Equal(10, nameProp!.MinLength);

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
        Console.WriteLine();
    }

}
