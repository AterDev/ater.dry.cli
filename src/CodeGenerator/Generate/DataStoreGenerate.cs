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
    /// 获取接口模板内容
    /// </summary>
    /// <param name="tplName"></param>
    /// <returns></returns>
    public string GetInterfaceFile(string tplName)
    {
        var content = GetTplContent($"Interface.{tplName}.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        return content;
    }

    /// <summary>
    /// 获取实现模板内容
    /// </summary>
    /// <param name="tplName"></param>
    /// <returns></returns>
    public string GetImplementFile(string tplName)
    {
        var content = GetTplContent($"Implement.{tplName}.tpl");
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
            $"global using {ServiceNamespace}.{Const.QUERY_STORE};",
            $"global using {ServiceNamespace}.{Const.COMMAND_STORE};",
            $"global using {ServiceNamespace}.Implement;",
            $"global using {ServiceNamespace}.Manager;",
            $"global using {ServiceNamespace}.IManager;",
            "global using Microsoft.Extensions.DependencyInjection;",
        };
    }

    /// <summary>
    /// 生成store实现
    /// </summary>
    /// <param name="queryOrCommand">Query or Command</param>
    /// <returns></returns>
    public string GetStoreContent(string queryOrCommand)
    {
        if (queryOrCommand != "Query" && queryOrCommand != "Command")
        {
            throw new ArgumentException("不允许的参数");
        }
        var contextName = queryOrCommand + "DbContext";
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        // 生成基础仓储实现类，替换模板变量并写入文件
        var tplContent = GetTplContent($"Implement.{queryOrCommand}StoreContent.tpl");
        tplContent = tplContent.Replace(TplConst.NAMESPACE, ServiceNamespace);
        //tplContent = tplContent.Replace(TplConst.SHARE_NAMESPACE, ShareNamespace);
        tplContent = tplContent.Replace(TplConst.DBCONTEXT_NAME, contextName);
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName);
        return tplContent;
    }
    /// <summary>
    /// store上下文
    /// </summary>
    /// <returns></returns>
    public string GetDataStoreContext()
    {
        // 获取所有继承了 DataStoreBase 的类
        var assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(StorePath));
        var cpl = new CompilationHelper(StorePath, assemblyName);
        var classes = cpl.GetAllClasses();

        string props = "";
        string ctorParams = "";
        string ctorAssign = "";
        if (classes != null)
        {
            string oneTab = "    ";
            string twoTab = "        ";

            var queryStores = CompilationHelper.GetClassNameByBaseType(classes, "QuerySet");
            var commandStores = CompilationHelper.GetClassNameByBaseType(classes, "CommandSet");

            var allDataStores = queryStores.Concat(commandStores);

            if (allDataStores.Any())
            {
                allDataStores.ToList().ForEach(dataStore =>
                {
                    // 属性名
                    var propName = dataStore.Name.Replace("Store","");
                    // 属性类型
                    var propType = dataStore.Name.EndsWith($"{Const.QUERY_STORE}")?"QuerySet":"CommandSet";
                    // 属性泛型
                    var propGeneric = dataStore.Name.Replace($"{Const.QUERY_STORE}","")
                    .Replace($"{Const.COMMAND_STORE}","");

                    var row = $"{oneTab}public {propType}<{propGeneric}> {propName} {{ get; init; }}";
                    props += row + Environment.NewLine;
                    // 构造函数参数
                    row = $"{twoTab}{dataStore.Name} {propName.ToCamelCase()},";
                    ctorParams += row + Environment.NewLine;
                    // 构造函数赋值
                    row = $"{twoTab}{propName} = {propName.ToCamelCase()};";
                    ctorAssign += row + Environment.NewLine;
                    ctorAssign += $"{twoTab}AddCache({propName});" + Environment.NewLine;
                });
            }
        }
        // 构建服务
        var content = GetTplContent("Implement.DataStoreContext.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        content = content.Replace(TplConst.STORECONTEXT_PROPS, props);
        content = content.Replace(TplConst.STORECONTEXT_PARAMS, ctorParams);
        content = content.Replace(TplConst.STORECONTEXT_ASSIGN, ctorAssign);
        return content;
    }

    /// <summary>
    /// 服务注册代码
    /// </summary>
    /// <returns></returns>
    public string GetStoreService()
    {
        var storeServiceContent = "";
        var managerServiceContent = "";

        // 获取所有data stores
        var storeDir = Path.Combine(StorePath, "DataStore");
        if (!Directory.Exists(storeDir)) return string.Empty;
        var files = Directory.GetFiles(storeDir, "*DataStore.cs", SearchOption.TopDirectoryOnly);

        var queryFiles = Directory.GetFiles(Path.Combine(StorePath,$"{Const.QUERY_STORE}"),$"*{Const.QUERY_STORE}.cs",SearchOption.TopDirectoryOnly);
        var commandFiles = Directory.GetFiles(Path.Combine(StorePath,$"{Const.COMMAND_STORE}"),$"*{Const.COMMAND_STORE}.cs",SearchOption.TopDirectoryOnly);

        files = files.Concat(queryFiles).Concat(commandFiles).ToArray();

        if (files != null)
        {
            files.ToList().ForEach(file =>
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var row = $"        services.AddScoped(typeof({name}));";
                storeServiceContent += row + Environment.NewLine;
            });
        }

        // 获取所有manager
        var managerDir = Path.Combine(StorePath, "Manager");
        if (!Directory.Exists(managerDir)) return string.Empty;
        files = Directory.GetFiles(managerDir, "*Manager.cs", SearchOption.TopDirectoryOnly);
        if (files != null)
        {
            files.ToList().ForEach(file =>
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var row = $"        services.AddScoped(typeof({name}));";
                managerServiceContent += row + Environment.NewLine;
            });
        }

        // 构建服务
        var content = GetTplContent("Implement.StoreServicesExtensions.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        content = content.Replace(TplConst.SERVICE_STORES, storeServiceContent);
        content = content.Replace(TplConst.SERVICE_MANAGER, managerServiceContent);
        return content;
    }

    /// <summary>
    /// Manager接口内容
    /// </summary>
    /// <returns></returns>
    public string GetIManagerContent()
    {
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        var tplContent = GetTplContent($"Implement.IManager.tpl");
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName);
        tplContent = tplContent.Replace(TplConst.NAMESPACE, ServiceNamespace);
        return tplContent;
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContext()
    {
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        var tplContent = GetTplContent($"Implement.Manager.tpl");
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName);
        tplContent = tplContent.Replace(TplConst.NAMESPACE, ServiceNamespace);
        return tplContent;
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
        content = content.Replace(TplConst.SERVICE_STORES, storeServiceDIContent);
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

    /// <summary>
    /// 添加依赖注入扩展方法
    /// </summary>
    /// <returns></returns>
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
