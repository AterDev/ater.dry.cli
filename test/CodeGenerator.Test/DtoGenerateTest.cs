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

        entityHelper.GetPropertyInfos("Blog");
        Console.WriteLine();
    }

}
