using CodeGenerator.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Droplet.CommandLine.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class RepositoryGenerate : GenerateBase
{
    public string EntityPath { get; set; }
    public string ServicePath { get; set; }
    public string SharePath { get; set; }
    public RepositoryGenerate(string entityPath, string servicePath)
    {
        EntityPath = entityPath;
        ServicePath = servicePath;
        SharePath = Config.SHARE_PATH;
    }

    /// <summary>
    /// 生成仓储
    /// </summary>
    public void GenerateReponsitory()
    {
        // 获取生成需要的实体名称
        if (!File.Exists(EntityPath))
        {
            return;
        }
        var content = File.ReadAllText(EntityPath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetCompilationUnitRoot();
        var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        string className = classDeclarationSyntax.Identifier.ToString();
        // 获取数据库上下文信息

        var cpl = new CompilationHelper(Config.DBCONTEXT_PATH, Config.DBCONTEXT_NAMESPACE);
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
        // 生成基础仓储实现类，替换模板变量并写入文件
        var tplContent = GetTplContent("Repository.tpl");

        tplContent = tplContent.Replace("{ServiceNamespace}", Config.SERVICE_NAMESPACE);
        tplContent = tplContent.Replace("{$ContextName}", contextName);
        SaveToFile(Path.Combine(ServicePath, "Repositories"), "Repository.cs", tplContent);
        // 生成当前实体的仓储服务
        tplContent = GetTplContent("MyRepository.tpl");
        tplContent = tplContent.Replace("{$EntityName}", className);
        tplContent = tplContent.Replace("{$ContextName}", contextName);
        SaveToFile(Path.Combine(ServicePath, "Repositories"), className + "Repository.cs", tplContent);
        Console.WriteLine("仓储生成完成");
    }


}
