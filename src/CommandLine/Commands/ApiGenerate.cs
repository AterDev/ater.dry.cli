using CodeGenerator.Infrastructure.Helper;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Droplet.CommandLine.Commands;

/// <summary>
/// 控制器代码生成
/// </summary>
public class ApiGenerate : GenerateBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; }
    /// <summary>
    /// service项目路径
    /// </summary>
    public string ServicePath { get; }
    /// <summary>
    /// Web站点路径
    /// </summary>
    public string WebPath { get; }

    public ApiGenerate(string entityPath, string servicePath, string webPath)
    {
        EntityPath = entityPath;
        ServicePath = servicePath;
        WebPath = webPath;
    }
    /// <summary>
    /// 生成控制器
    /// </summary>
    public void GenerateController()
    {
        // 在Web项目中创建基类
        var tplContent = GetTplContent("ApiController.tpl");
        // 如果已经存在，则不创建
        if (!File.Exists(Path.Combine(WebPath, "Controllers", "ApiController.cs")))
        {
            // 替换数据库上下文
            var contextPath = Path.Combine(ServicePath, "..", "Data.Context");
            var cpl = new CompilationHelper(contextPath, "Data.Context");
            var classes = cpl.GetAllClasses();
            if (classes == null)
            {
                return;
            }
            var contextClass = cpl.GetClassNameByBaseType(classes, "IdentityDbContext")?.FirstOrDefault();
            if (contextClass == null)
            {
                contextClass = cpl.GetClassNameByBaseType(classes, "DbContext").FirstOrDefault();
            }
            var contextName = contextClass?.Name ?? "ContextBase";
            tplContent = tplContent.Replace("{$ContextName}", contextName);
            SaveToFile(Path.Combine(WebPath, "Controllers"), "ApiController.cs", tplContent);
            Console.WriteLine("写入控制器基类完成");
        }
        else
        {
            Console.WriteLine("Already exist ApiController.cs!");
        }

        // 在Web项目中生成相应控制器
        tplContent = GetTplContent("MyController.tpl");
        // 查找模型名称和描述
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine("Not exist " + EntityPath);
        }
        // 解析类名
        var content = File.ReadAllText(EntityPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetCompilationUnitRoot();
        var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        string className = classDeclarationSyntax.Identifier.ToString();
        // 替换模板
        tplContent = tplContent.Replace("{$Namespace}", new DirectoryInfo(WebPath).Name);
        tplContent = tplContent.Replace("{$EntityName}", className).Replace("{$Description}", className);
        SaveToFile(Path.Combine(WebPath, "Controllers"), className + "Controller.cs", tplContent);
        Console.WriteLine("写入Api控制器完成");
    }
    /// <summary>
    /// 生成仓储的注入服务
    /// </summary>
    public void GenerateRepositoryServicesDI()
    {
        // 获取services中所有Repository仓储类
        var dir = new DirectoryInfo(Path.Combine(ServicePath, "Repositories"));
        Console.WriteLine("搜索目录:" + dir.FullName);
        var files = dir.GetFiles("*Repository.cs", SearchOption.TopDirectoryOnly);
        var classes = files.Where(f => f.Name != "Repository.cs").ToList();
        Console.WriteLine("共找到" + classes.Count + "个仓储");
        var content = string.Join(string.Empty, classes.Select(c => "            services.AddScoped(typeof(" + Path.GetFileNameWithoutExtension(c.FullName) + "));\r\n").ToArray());
        // 替换模板文件并写入
        var tplContent = GetTplContent("RepositoryServiceExtensions.tpl");
        string replaceSign = "// {$TobeAddRepository}";
        tplContent = tplContent.Replace(replaceSign, content);
        File.WriteAllText(Path.Combine(WebPath, "RepositoryServiceExtensions.cs"), tplContent);
        Console.WriteLine("create file:" + Path.Combine(WebPath, "RepositoryServiceExtensions.cs") + "\r\n" + "写入仓储注册服务完成");
    }
}
