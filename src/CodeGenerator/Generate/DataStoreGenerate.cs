
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class DataStoreGenerate : GenerateBase
{
    /// <summary>
    /// Entity 文件路径
    /// </summary>
    public string EntityPath { get; set; }
    /// <summary>
    /// DataStroe所在项目目录路径
    /// </summary>
    public string ServicePath { get; set; }
    /// <summary>
    /// DTO 所有项目目录路径
    /// </summary>
    public string SharePath { get; set; }
    public string? ContextName { get; set; }
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; }
    public string? ServiceNamespace { get; set; }
    public DataStoreGenerate(string entityPath, string dtoPath, string servicePath, string? contextName = null)
    {
        EntityPath = entityPath;
        ServicePath = servicePath;
        SharePath = dtoPath;
        ContextName = contextName;
        ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(SharePath));
        ServiceNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ServicePath));
    }

    /// <summary>
    /// 接口文件内容
    /// </summary>
    /// <returns></returns>
    public string GetStoreInterface()
    {
        var content = GetTplContent("Interface.IDataStore.tpl");
        content = content.Replace(TplConstant.NAMESPACE, ServiceNamespace);
        return content;
    }

    /// <summary>
    /// 接口实现类
    /// </summary>
    /// <returns></returns>
    public string GetStoreBase()
    {
        var content = GetTplContent("Implement.DataStoreBase.tpl");
        content = content.Replace(TplConstant.NAMESPACE, ServiceNamespace);
        return content;
    }

    /// <summary>
    /// 命名空间
    /// </summary>
    /// <returns></returns>
    public List<string> GetGlobalUsings()
    {
        return new List<string>
        {
            "global using Microsoft.Extensions.DependencyInjection;",
            "global using Microsoft.Extensions.Logging;",
            $"global using {ServiceNamespace}.Interface;",
            $"global using {ServiceNamespace}.DataStore;",
            $"global using {ShareNamespace}.Models;"
        };
    }

    /// <summary>
    /// 自定义内容
    /// </summary>
    /// <returns></returns>
    public string GetStoreContent()
    {
        var contextName = GetContextName(ContextName);
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        // 生成基础仓储实现类，替换模板变量并写入文件
        var tplContent = GetTplContent("Implement.DataStore.tpl");
        tplContent = tplContent.Replace(TplConstant.NAMESPACE, ServiceNamespace);
        tplContent = tplContent.Replace(TplConstant.SHARE_NAMESPACE, ShareNamespace);
        tplContent = tplContent.Replace(TplConstant.DBCONTEXT_NAME, contextName);
        tplContent = tplContent.Replace(TplConstant.ENTITY_NAME, entityName);
        return tplContent;
    }
    /// <summary>
    /// 服务注册
    /// </summary>
    /// <param name="dataStores">all store names</param>
    /// <returns></returns>
    public string GetStoreService(List<string> dataStores)
    {
        var dataStoreContent = "";
        dataStores.ForEach(dataStore =>
        {
            var row = $"        services.AddScoped(typeof({dataStore}DataStore));{Environment.NewLine}";
            dataStoreContent += row;
        });
        // 构建服务
        var content = GetTplContent("Implement.DataStoreExtensioins.tpl");
        content = content.Replace(TplConstant.NAMESPACE, ServiceNamespace);
        content = content.Replace(TplConstant.DATASTORE_SERVICES, dataStoreContent);
        return content;
    }

    /// <summary>
    /// get user DbContext name
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    protected string GetContextName(string? contextName = null)
    {
        var name = "ContextBase";
        var assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(ServicePath));
        var cpl = new CompilationHelper(ServicePath, assemblyName);
        var classes = cpl.GetAllClasses();
        if (classes != null)
        {
            // 获取所有继承 dbcontext的上下文
            var allDbContexts = cpl.GetClassNameByBaseType(classes, "IdentityDbContext");
            if (allDbContexts == null)
                allDbContexts = cpl.GetClassNameByBaseType(classes, "DbContext");

            if (allDbContexts != null)
            {
                if (contextName == null)
                {
                    name = allDbContexts.FirstOrDefault()!.Name;
                }
                else if (allDbContexts.Any(c => c.Name.Equals(contextName)))
                {
                    name = contextName;
                }
            }
        }
        return name;
    }


}
