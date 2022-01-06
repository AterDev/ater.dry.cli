using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeGenerator.Infrastructure.Helper;

public class AssemblyHelper
{

    public static FileInfo? FindProjectFile(DirectoryInfo dir, DirectoryInfo root)
    {
        if (dir.FullName == root.FullName) return default;
        var file = dir.GetFiles("*.csproj").FirstOrDefault();
        if (file == null)
        {
            return FindProjectFile(dir.Parent!, root);
        }
        return file;
    }


    /// <summary>
    /// 在项目中寻找文件
    /// </summary>
    /// <param name="projectFilePath"></param>
    /// <param name="searchFileName"></param>
    /// <returns>the search file path,return null if not found </returns>
    public static string? FindFileInProject(string projectFilePath, string searchFileName)
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(projectFilePath));
        var files = Directory.GetFiles(dir.FullName, searchFileName, SearchOption.AllDirectories);
        if (files != null)
        {
            return files[0];
        }
        return default;
    }

    /// <summary>
    /// 解析项目文件xml 获取名称,没有自定义则取文件名
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GetAssemblyName(FileInfo file)
    {
        var xml = XElement.Load(file.FullName);
        var nodes = xml.Descendants("PropertyGroup")
            .Where(pg => pg.Elements().Any(e => e.Name.LocalName.Equals("AssemblyName")))
            .ToList();
        if (nodes != null && nodes.Count > 0)
        {
            return nodes.First().Value;
        }
        return Path.GetFileNameWithoutExtension(file.Name);

    }
}
