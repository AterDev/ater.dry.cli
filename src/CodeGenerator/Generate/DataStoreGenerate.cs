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
    public string StorePath { get; set; }
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
        SharePath = dtoPath;
        StorePath = servicePath;
        ContextName = contextName;
        ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(SharePath));
        ServiceNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(StorePath));
    }

    /// <summary>
    /// 接口文件内容
    /// </summary>
    /// <returns></returns>
    public string GetStoreInterface()
    {
        var content = GetTplContent("Interface.IDataStore.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        return content;
    }

    public string GetUserContextInterface()
    {
        var content = GetTplContent("Interface.IUserContext.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        return content;
    }
    public string GetUserContextClass()
    {
        var content = GetTplContent("Implement.UserContext.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        return content;
    }

    /// <summary>
    /// 接口实现类
    /// </summary>
    /// <returns></returns>
    public string GetStoreBase()
    {
        var content = GetTplContent("Implement.DataStoreBase.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace)
            .Replace(TplConst.ID_TYPE, Config.IdType)
            .Replace(TplConst.CREATEDTIME_NAME, Config.CreatedTimeName);
        return content;
    }

    /// <summary>
    /// 全局依赖
    /// </summary>
    /// <returns></returns>
    public List<string> GetGlobalUsings()
    {
        var fileInfo = new FileInfo(EntityPath);
        var projectFile = AssemblyHelper.FindProjectFile(fileInfo.Directory!, fileInfo.Directory!.Root);
        var entityNamepapce = AssemblyHelper.GetNamespaceName(projectFile!.Directory!);
        return new List<string>
        {
            "global using System;",
            "// global using EntityFramework;",
            "global using Microsoft.EntityFrameworkCore;",
            "global using Microsoft.Extensions.Logging;",
            $"global using {entityNamepapce}.Utils;",
            $"global using {entityNamepapce}.Models;",
            $"// global using {entityNamepapce}.Identity;",
            $"global using {ShareNamespace}.Models;",
            $"global using {ServiceNamespace}.Interface;",
            $"global using {ServiceNamespace}.DataStore;",
            "global using Microsoft.Extensions.DependencyInjection;",
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
        tplContent = tplContent.Replace(TplConst.NAMESPACE, ServiceNamespace);
        tplContent = tplContent.Replace(TplConst.SHARE_NAMESPACE, ShareNamespace);
        tplContent = tplContent.Replace(TplConst.DBCONTEXT_NAME, contextName);
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName);
        return tplContent;
    }
    /// <summary>
    /// 服务注册代码
    /// </summary>
    /// <returns></returns>
    public string GetStoreService()
    {
        var storeServiceDIContent = "";
        var storeDir = Path.Combine(StorePath, "DataStore");
        if (!Directory.Exists(storeDir)) return string.Empty;
        var files = Directory.GetFiles(storeDir, "*DataStore.cs", SearchOption.TopDirectoryOnly);
        if (files != null)
        {
            files.ToList().ForEach(file =>
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var row = $"        services.AddScoped(typeof({name}));";
                storeServiceDIContent += row + Environment.NewLine;
            });
        }
        // 构建服务
        var content = GetTplContent("Implement.DataStoreExtensioins.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        content = content.Replace(TplConst.DATASTORE_SERVICES, storeServiceDIContent);
        return content;
    }

    /// <summary>
    /// 服务注册
    /// </summary>
    /// <param name="dataStores">all store names</param>
    /// <returns></returns>
    public string GetStoreServiceAfterBuild()
    {
        var storeServiceDIContent = "";
        // 获取所有继承了 DataStoreBase 的类
        var assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(StorePath));
        var cpl = new CompilationHelper(StorePath, assemblyName);
        var classes = cpl.GetAllClasses();
        if (classes != null)
        {
            var allDataStores = CompilationHelper.GetClassNameByBaseType(classes, "DataStoreBase");
            if (allDataStores.Any())
            {
                allDataStores.ToList().ForEach(dataStore =>
                {
                    var row = $"        services.AddScoped(typeof({dataStore.Name}));";
                    storeServiceDIContent += row + Environment.NewLine;
                });
            }
        }
        // 构建服务
        var content = GetTplContent("Implement.DataStoreExtensioins.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        content = content.Replace(TplConst.DATASTORE_SERVICES, storeServiceDIContent);
        return content;
    }

    /// <summary>
    /// get user DbContext name
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public string GetContextName(string? contextName = null)
    {
        var name = "ContextBase";
        var assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(StorePath));
        var cpl = new CompilationHelper(StorePath, assemblyName);
        var classes = cpl.GetAllClasses();
        if (classes != null)
        {
            // 获取所有继承 dbcontext的上下文
            var allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "IdentityDbContext");
            if (!allDbContexts.Any())
                allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "DbContext");

            //Console.WriteLine("find dbcontext:" + allDbContexts.FirstOrDefault().Name);
            if (allDbContexts.Any())
            {
                if (string.IsNullOrEmpty(contextName))
                {
                    name = allDbContexts.FirstOrDefault()!.Name;
                }
                else if (allDbContexts.ToList().Any(c => c.Name.Equals(contextName)))
                {
                    Console.WriteLine("find contextName:" + contextName);
                    name = contextName;
                }
            }
        }
        Console.WriteLine("the contextName:" + name);
        return name;
    }

    public string GetExtensions()
    {
        var entityDir =  new FileInfo(EntityPath).Directory!;
        var entityProjectFile = AssemblyHelper.FindProjectFile(entityDir, entityDir.Root);
        var entityNamespace = AssemblyHelper.GetNamespaceName(entityProjectFile!.Directory!);
        var tplContent = GetTplContent("Extensions.tpl");
        tplContent = tplContent.Replace(TplConst.NAMESPACE, entityNamespace);
        return tplContent;
    }
}
