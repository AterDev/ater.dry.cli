using Core.Entities;

namespace CodeGenerator.Generate;

/// <summary>
/// 生成Rest API控制器
/// </summary>
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
            $"global using {EntityNamespace}.Models;",
            $"global using {ServiceNamespace}.IManager;",
            $"global using {ServiceNamespace}.Const;",
        };
    }

    /// <summary>
    /// 生成控制器
    /// </summary>
    public string GetRestApiContent()
    {
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
        string tplContent = GetTplContent("Implement.RestControllerContent.tpl");

        // 依赖注入
        string additionManagerProps = "";
        string additionManagerDI = "";
        string additionManagerInit = "";

        var requiredNavigations = EntityInfo.GetRequiredNavigation();
        requiredNavigations?.ForEach(navigation =>
        {
            var name = navigation.Type;

            additionManagerProps += $"    private readonly I{name}Manager _{name.ToCamelCase()}Manager;" + Environment.NewLine;

            additionManagerDI += $",{Environment.NewLine}        I{name}Manager {name.ToCamelCase()}Manager";
            //_catalogManager = catalogManager;
            additionManagerInit += $"        _{name.ToCamelCase()}Manager = {name.ToCamelCase()}Manager;" + Environment.NewLine;
        });
        tplContent = tplContent.Replace("${AdditionManagersProps}", additionManagerProps)
            .Replace("${AdditionManagersDI}", additionManagerDI)
            .Replace("${AdditionManagersInit}", additionManagerInit);

        var addContent = GetAddApiContent();
        var updateContent = GetUpdateApiContent();

        tplContent = tplContent.Replace("${AddActionBlock}", addContent)
            .Replace("${UpdateActionBlock}", updateContent);

        // add see cref comment
        var comment = EntityInfo?.Comment + Environment.NewLine + $"/// <see cref=\"{ServiceNamespace}.Manager.{entityName}Manager\"/>";
        tplContent = tplContent.Replace(TplConst.NAMESPACE, ApiNamespace)
            .Replace(TplConst.SHARE_NAMESPACE, ShareNamespace)
            .Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.API_SUFFIX, Suffix)
            .Replace(TplConst.ID_TYPE, Config.IdType)
            .Replace(TplConst.COMMENT, comment);

        // 清理模板未被替换的变量
        return ClearTemplate(tplContent);
    }

    /// <summary>
    /// 生成添加方法
    /// </summary>
    /// <returns></returns>
    public string? GetAddApiContent()
    {
        string content = "";
        string entityName = EntityInfo.Name;
        var requiredNavigations = EntityInfo.GetRequiredNavigation();

        requiredNavigations?.ForEach(nav =>
        {
            var manager = "_" + nav.Type.ToCamelCase() + "Manager";
            // 如果关联的是用户
            content += nav.Type switch
            {
                not "User" and not "SystemUser" => $$"""
                        if (!await {{manager}}.ExistAsync(dto.{{nav.Type}}Id))
                        {
                            return NotFound("不存在的{{nav.CommentSummary ?? nav.Type}}");
                        }

                """,
                _ => $$"""
                        if (!await _user.ExistAsync()) { return NotFound(ErrorMsg.NotFoundUser); }

                """,
            };
        });
        content += """
                    var entity = await manager.CreateNewEntityAsync(dto);
                    return await manager.AddAsync(entity);
            """;
        return content;
    }

    /// <summary>
    /// 生成更新方法
    /// </summary>
    /// <returns></returns>
    public string? GetUpdateApiContent()
    {
        string content = """
                    var current = await manager.GetCurrentAsync(id);
                    if (current == null) { return NotFound(ErrorMsg.NotFoundResource); };

            """;
        string entityName = EntityInfo.Name;
        var requiredNavigations = EntityInfo.GetRequiredNavigation();

        requiredNavigations?.ForEach(nav =>
        {
            var manager = "_" + nav.Type.ToCamelCase() + "Manager";
            var variable = nav.Type.ToCamelCase();
            if (!nav.Type.Equals("User") && !nav.Type.Equals("SystemUser"))
            {
                content += $$"""
                        if (current.{{nav.Name}}.Id != dto.{{nav.Type}}Id)
                        {
                            var {{variable}} = await {{manager}}.GetCurrentAsync(dto.{{nav.Type}}Id);
                            if ({{variable}} == null) { return NotFound("不存在的{{nav.CommentSummary ?? nav.Type}}"); }
                            current.{{nav.Name}} = {{variable}};
                        }

                """;
            }
        });
        content += """
                    return await manager.UpdateAsync(current, dto);
            """;
        return content;
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

    private string ClearTemplate(string tplContent)
    {
        return tplContent.Replace("${AdditionManagersProps}", "")
            .Replace("${AdditionManagersDI}", "")
            .Replace("${AdditionManagersInit}", "");
    }
}
