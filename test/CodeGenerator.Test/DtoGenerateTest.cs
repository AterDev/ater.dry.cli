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
        var filePath = @"C:\self\cli\test\CodeGenerator.Test\Entity\Comments.cs";
        var fileInfo = new FileInfo(filePath);
        var root = fileInfo.Directory.Root;
        var dir = fileInfo.Directory;

        var projectFile = Search(dir, root);

        Console.WriteLine(projectFile);
        string? Search(DirectoryInfo dir, DirectoryInfo root)
        {
            if (dir.FullName == root.FullName) return default;
            var file = dir.GetFiles("*.csproj").FirstOrDefault();
            if (file == null)
            {
                return Search(dir.Parent, root);
            }
            return file.Name;
        }
    }

}
