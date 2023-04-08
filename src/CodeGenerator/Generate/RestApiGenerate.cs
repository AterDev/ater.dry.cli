namespace CodeGenerator.Generate;

public class RestApiGenerate : GenerateBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; }
    /// <summary>
    /// DTO 所有项目目录路径
    /// </summary>
    public string SharePath { get; set; }
    /// <summary>
    /// service项目目录路径
    /// </summary>
    public string StorePath { get; }
    /// <summary>
    /// api项目目录路径
    /// </summary>
    public string ApiPath { get; }
    /// <summary>
    /// API后缀
    /// </summary>
    public string? Suffix { get; set; }
    public string? EntityNamespace { get; set; }
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; }
    public string? ServiceNamespace { get; set; }
    public string? ApiNamespace { get; set; }
    public readonly EntityInfo EntityInfo;

    public RestApiGenerate(string entityPath, string dtoPath, string servicePath, string apiPath, string? suffix = null)
    {
        EntityPath = entityPath;
        SharePath = dtoPath;
        StorePath = servicePath;
        ApiPath = apiPath;
        Suffix = suffix;
        DirectoryInfo entityDir = new FileInfo(entityPath).Directory!;
        FileInfo? entityProjectFile = AssemblyHelper.FindProjectFile(entityDir, entityDir.Root);
        if (entityProjectFile == null)
        {
            throw new FileNotFoundException("project file not found!");
        }

        EntityNamespace = AssemblyHelper.GetNamespaceName(entityProjectFile.Directory!);
        ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(SharePath));
        ServiceNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(StorePath));
        ApiNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ApiPath));

        EntityParseHelper entityHelper = new(entityPath);
        EntityInfo = entityHelper.GetEntity();
    }

    public string GetRestApiInterface()
    {
        string content = GetTplContent("Interface.IRestController.tpl");
        content = content.Replace(TplConst.NAMESPACE, ApiNamespace);
        return content;
    }

    /// <summary>
    /// Rest Api 实现类
    /// </summary>
    /// <param name="dbContextName">DbContext 名称</param>
    /// <returns></returns>
    public string GetRestApiBase()
    {
        string dbContextName = GetContextName();
        string content = GetTplContent("Implement.RestControllerBase.tpl");
        content = content.Replace(TplConst.NAMESPACE, ApiNamespace)
            .Replace(TplConst.DBCONTEXT_NAME, dbContextName);
        return content;
    }

    public List<string> GetGlobalUsings()
    {
        return new List<string>
        {
            "global using Microsoft.Extensions.DependencyInjection;",
            "global using Microsoft.AspNetCore.Mvc;",
            "global using Microsoft.AspNetCore.Authorization;",
            "global using System.Text.Json.Serialization;",
            "global using Microsoft.EntityFrameworkCore;",
            "global using Http.API.Infrastructure;",
            $"global using {EntityInfo.NamespaceName};",
            $"global using {EntityNamespace}.Utils;",
            $"global using {EntityNamespace}.Models;",
            $"global using {ShareNamespace}.Models;",
            $"global using {ServiceNamespace}.Interface;",
            $"global using {ServiceNamespace}.IManager;",
        };
    }

    /// <summary>
    /// 生成控制器
    /// </summary>
    public string GetRestApiContent()
    {
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
        string tplContent = GetTplContent("Implement.RestControllerContent.tpl");

        //var actionContent = GetAddApiContent();
        //actionContent += GetUpdateApiContent();

        tplContent = tplContent.Replace(TplConst.NAMESPACE, ApiNamespace)
            .Replace(TplConst.SHARE_NAMESPACE, ShareNamespace)
            .Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.API_SUFFIX, Suffix)
            .Replace(TplConst.ID_TYPE, Config.IdType)
            .Replace(TplConst.COMMENT, EntityInfo?.Comment ?? "");
        //.Replace(TplConst.ADDITION_ACTION, actionContent ?? "")
        //.Replace(TplConst.ID_TYPE, Config.IdType);
        return tplContent;
    }

    /// <summary>
    /// 生成关联添加
    /// </summary>
    /// <returns></returns>
    public string? GetAddApiContent()
    {
        string entityName = EntityInfo.Name;
        Core.Models.PropertyInfo? navigationProp = EntityInfo.GetNavigation();
        if (navigationProp == null)
        {
            return null;
        }

        string content = $@"
    /// <summary>
    /// 关联添加
    /// </summary>
    /// <param name=""id"">所属对象id</param>
    /// <param name=""list"">数组</param>
    /// <param name=""dependStore""></param>
    /// <returns></returns>
    [HttpPost(""{{id}}"")]
    public async Task<ActionResult<int>> AddInAsync([FromRoute] ${{IdType}} id, List<{entityName}UpdateDto> list, [FromServices] {navigationProp.Type}DataStore dependStore)
    {{
        var depend = await dependStore.FindAsync(id);
        if (depend == null) return NotFound(""depend not exist"");
        var newList = new List<{entityName}>();
        list.ForEach(item =>
        {{
            var newItem = new {entityName}()
            {{
                {navigationProp.Name} = depend
            }};
            newList.Add(newItem.Merge(item));
        }});
        return await _store.BatchAddAsync(newList);
    }}";

        return content;
    }
    // TODO:update api 
    public static string? GetUpdateApiContent()
    {
        return default;
    }

    /// <summary>
    /// 生成仓储的注入服务
    /// </summary>
    public void GenerateRepositoryServicesDI()
    {
        // 获取services中所有Repository仓储类
        DirectoryInfo dir = new(Path.Combine(StorePath, "Repositories"));
        Console.WriteLine("搜索目录:" + dir.FullName);
        FileInfo[] files = dir.GetFiles("*Repository.cs", SearchOption.TopDirectoryOnly);
        List<FileInfo> classes = files.Where(f => f.Name != "Repository.cs").ToList();
        Console.WriteLine("共找到" + classes.Count + "个仓储");
        string content = string.Join(string.Empty, classes.Select(c => "            services.AddScoped(typeof(" + Path.GetFileNameWithoutExtension(c.FullName) + "));\r\n").ToArray());
        // 替换模板文件并写入
        string tplContent = GetTplContent("RepositoryServiceExtensions.tpl");
        string replaceSign = "// {$TobeAddRepository}";
        tplContent = tplContent.Replace(replaceSign, content);
        File.WriteAllText(Path.Combine(ApiPath, "RepositoryServiceExtensions.cs"), tplContent);
        Console.WriteLine("create file:" + Path.Combine(ApiPath, "RepositoryServiceExtensions.cs") + "\r\n" + "写入仓储注册服务完成");
    }


    /// <summary>
    /// get user DbContext name
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public string GetContextName(string? contextName = null)
    {
        string name = "ContextBase";
        string? assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(StorePath));
        CompilationHelper cpl = new(StorePath, assemblyName);
        IEnumerable<INamedTypeSymbol> classes = cpl.AllClass;
        if (classes != null)
        {
            // 获取所有继承 dbcontext的上下文
            IEnumerable<INamedTypeSymbol> allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "IdentityDbContext");
            if (!allDbContexts.Any())
            {
                allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "DbContext");
            }

            if (allDbContexts.Any())
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
