using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeGenerator.Infrastructure.Helper;

public class AssemblyHelper
{
    /// <summary>
    /// 搜索项目文件,直到根目录
    /// </summary>
    /// <param name="dir">起始目录</param>
    /// <param name="root">根目录</param>
    /// <returns></returns>
    public static FileInfo? FindProjectFile(DirectoryInfo dir, DirectoryInfo? root = null)
    {
        var file = dir.GetFiles("*.csproj").FirstOrDefault();
        if (root == null)
        {
            return file;
        }
        if (file == null && dir != root)
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
        if (files.Any())
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
        var node = xml.Descendants("PropertyGroup")
            .SelectMany(pg => pg.Elements())
            .Where(el => el.Name.LocalName.Equals("AssemblyName"))
            .FirstOrDefault();
        // 默认名称
        var name = Path.GetFileNameWithoutExtension(file.Name);
        if (node != null)
        {
            if (!node.Value.Contains("$(MSBuildProjectName)"))
            {
                name = node.Value;
            }
        }
        return name;
    }

    public static string? GetAssemblyName(DirectoryInfo dir)
    {
        var file = FindProjectFile(dir);
        if (file == null) return null;
        return GetAssemblyName(file);
    }
    /// <summary>
    /// 获取命名空间名称， 不支持MSBuildProjectName表达式
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string? GetNamespaceName(DirectoryInfo dir)
    {
        var file = FindProjectFile(dir);
        if (file == null) return null;
        var xml = XElement.Load(file.FullName);
        var node = xml.Descendants("PropertyGroup")
            .SelectMany(pg => pg.Elements())
            .Where(el => el.Name.LocalName.Equals("RootNamespace"))
            .FirstOrDefault();
        // 默认名称
        var name = Path.GetFileNameWithoutExtension(file.Name);
        if (node != null)
        {
            if (!node.Value.Contains("MSBuildProjectName"))
            {
                name = node.Value;
            }
        }
        return name;
    }
}
