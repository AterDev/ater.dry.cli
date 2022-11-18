﻿using System.Xml.Linq;

namespace Core.Infrastructure.Helper;

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
        FileInfo? file = dir.GetFiles("*.csproj")?.FirstOrDefault();
        return root == null ? file : file == null && dir != root ? FindProjectFile(dir.Parent!, root) : file;
    }

    /// <summary>
    /// 在项目中寻找文件
    /// </summary>
    /// <param name="projectFilePath"></param>
    /// <param name="searchFileName"></param>
    /// <returns>the search file path,return null if not found </returns>
    public static string? FindFileInProject(string projectFilePath, string searchFileName)
    {
        DirectoryInfo dir = new(Path.GetDirectoryName(projectFilePath));
        string[] files = Directory.GetFiles(dir.FullName, searchFileName, SearchOption.AllDirectories);
        return files.Any() ? files[0] : default;
    }

    /// <summary>
    /// 解析项目文件xml 获取名称,没有自定义则取文件名
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static string GetAssemblyName(FileInfo file)
    {
        XElement xml = XElement.Load(file.FullName);
        XElement? node = xml.Descendants("PropertyGroup")
            .SelectMany(pg => pg.Elements())
            .Where(el => el.Name.LocalName.Equals("AssemblyName"))
            .FirstOrDefault();
        // 默认名称
        string name = Path.GetFileNameWithoutExtension(file.Name);
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
        FileInfo? file = FindProjectFile(dir);
        return file == null ? null : GetAssemblyName(file);
    }
    /// <summary>
    /// 获取命名空间名称， 不支持MSBuildProjectName表达式
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string? GetNamespaceName(DirectoryInfo dir)
    {
        FileInfo? file = FindProjectFile(dir);
        if (file == null)
        {
            return null;
        }

        XElement xml = XElement.Load(file.FullName);
        XElement? node = xml.Descendants("PropertyGroup")
            .SelectMany(pg => pg.Elements())
            .Where(el => el.Name.LocalName.Equals("RootNamespace"))
            .FirstOrDefault();
        // 默认名称
        string name = Path.GetFileNameWithoutExtension(file.Name);
        if (node != null)
        {
            if (!node.Value.Contains("MSBuildProjectName"))
            {
                name = node.Value;
            }
        }
        return name;
    }

    /// <summary>
    /// 获取解决方案文件
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="searchPattern"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    public static FileInfo? GetSlnFile(DirectoryInfo dir, string searchPattern, DirectoryInfo? root = null)
    {
        try
        {
            FileInfo? file = dir.GetFiles("*.sln")?.FirstOrDefault();
            return root == null ? file
                : file == null && dir != root ? GetSlnFile(dir.Parent!, searchPattern, root) : file;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// 获取当前项目下的 xml 注释中的members
    /// </summary>
    /// <returns></returns>
    public static List<XmlCommentMember>? GetXmlMembers(DirectoryInfo dir)
    {
        FileInfo? projectFile = dir.GetFiles("*.csproj")?.FirstOrDefault();
        if (projectFile != null)
        {
            string assemblyName = GetAssemblyName(projectFile);
            FileInfo? xmlFile = dir.GetFiles($"{assemblyName}.xml", SearchOption.AllDirectories).FirstOrDefault();
            if (xmlFile != null)
            {
                XElement xml = XElement.Load(xmlFile.FullName);
                List<XmlCommentMember> members = xml.Descendants("member")
                    .Select(s => new XmlCommentMember
                    {
                        FullName = s.Attribute("name")?.Value ?? "",
                        Summary = s.Element("summary")?.Value

                    }).ToList();
                return members;
            }
        }
        return null;
    }

    public class XmlCommentMember
    {
        public string FullName { get; set; } = string.Empty;
        public string? Summary { get; set; }
    }
}