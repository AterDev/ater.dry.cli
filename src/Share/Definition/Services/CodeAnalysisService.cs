using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Share.Share;

namespace Share.Services;
/// <summary>
/// 代码解析服务
/// </summary>
public class CodeAnalysisService
{
    public static List<EntityFile> GetEntityFiles(List<string> filePaths)
    {
        var entityFiles = new ConcurrentBag<EntityFile>();

        Parallel.ForEach(filePaths, path =>
        {
            string content = File.ReadAllText(path);
            var compilation = new CompilationHelper(path);
            compilation.LoadContent(content);

            if (compilation.IsEntityClass())
            {
                entityFiles.Add(new EntityFile
                {
                    Name = Path.GetFileName(path),
                    Path = path,
                });
            }
        });

        return [.. entityFiles];
    }


    public static List<string> GetEntityFilePaths(string entityAssemblyPath)
    {
        return Directory.GetFiles(entityAssemblyPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !(f.EndsWith(".g.cs")
                    || f.EndsWith(".AssemblyAttributes.cs")
                    || f.EndsWith(".AssemblyInfo.cs")
                    || f.EndsWith("GlobalUsings.cs")
                    || f.EndsWith("Modules.cs"))
                    ).ToList();
    }

}
