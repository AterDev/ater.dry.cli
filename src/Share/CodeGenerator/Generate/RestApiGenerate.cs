namespace CodeGenerator.Generate;

/// <summary>
/// 生成Rest API控制器
/// </summary>
public class RestApiGenerate
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
    public string ApplicationPath { get; }
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
    public string? ApplicationNamespace { get; set; }
    public string? ApiNamespace { get; set; }
    public readonly EntityInfo EntityInfo;

    public RestApiGenerate(string entityPath, string dtoPath, string applicationPath, string apiPath, string? suffix = null)
    {
        EntityPath = entityPath;
        SharePath = dtoPath;
        ApplicationPath = applicationPath;
        ApiPath = apiPath;
        Suffix = suffix;
        DirectoryInfo entityDir = new FileInfo(entityPath).Directory!;
        FileInfo? entityProjectFile = AssemblyHelper.FindProjectFile(entityDir, entityDir.Root) ?? throw new FileNotFoundException("project file not found!");

        if (Config.IsMicroservice)
        {
            EntityNamespace = Config.ServiceName + ".Definition.Entity";
            ShareNamespace = Config.ServiceName + ".Definition.Share";
            ApplicationNamespace = Config.ServiceName + ".Application";
            ApiNamespace = Config.ServiceName;
        }
        else
        {
            EntityNamespace = AssemblyHelper.GetNamespaceName(entityProjectFile.Directory!);
            ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(SharePath));
            ApplicationNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ApplicationPath));
            ApiNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ApiPath));
        }

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
        return
        [
            "global using Microsoft.Extensions.DependencyInjection;",
            "global using Microsoft.AspNetCore.Mvc;",
            "global using Microsoft.AspNetCore.Authorization;",
            "global using System.Text.Json.Serialization;",
            "global using Microsoft.EntityFrameworkCore;",
            "global using Ater.Web.Core.Models;",
            "global using Ater.Web.Core.Utils;",
            "global using Ater.Web.Abstraction;",
            "global using Ater.Web.Abstraction.Interface;",
            "global using Ater.Web.Extension.Services;",
            $"global using {EntityInfo.NamespaceName};",
            $"global using {ApplicationNamespace}.Manager;",
        ];
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

        List<Entity.PropertyInfo>? requiredNavigations = EntityInfo.GetRequiredNavigation()?
            .DistinctBy(n => n.Type).ToList();

        requiredNavigations?.ForEach(navigation =>
        {
            string name = navigation.Type;
            additionManagerProps += $"    private readonly {name}Manager _{name.ToCamelCase()}Manager = {name.ToCamelCase()}Manager;" + Environment.NewLine;
            additionManagerDI += $"  {name}Manager {name.ToCamelCase()}Manager,{Environment.NewLine}";

        });
        tplContent = tplContent.Replace(TplConst.ADDICTION_MANAGER_PROPS, additionManagerProps)
            .Replace(TplConst.ADDICTION_MANAGER_DI, additionManagerDI);


        string? addContent = GetAddApiContent();
        string? updateContent = GetUpdateApiContent();

        tplContent = tplContent.Replace(TplConst.ADD_ACTION_BLOCK, addContent)
            .Replace(TplConst.UPDATE_ACTION_BLOCK, updateContent);

        // add see cref comment
        string comment = EntityInfo?.Comment + Environment.NewLine + $"/// <see cref=\"{ApplicationNamespace}.Manager.{entityName}Manager\"/>";
        tplContent = tplContent.Replace(TplConst.NAMESPACE, ApiNamespace)
            .Replace(TplConst.SHARE_NAMESPACE, ShareNamespace)
            .Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.API_SUFFIX, Suffix)
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
        List<Entity.PropertyInfo>? requiredNavigations = EntityInfo.GetRequiredNavigation();

        requiredNavigations?.ForEach(nav =>
        {
            string manager = "_" + nav.Type.ToCamelCase() + "Manager";
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
                    var entity = await _manager.CreateNewEntityAsync(dto);
                    return await _manager.AddAsync(entity);
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
                    var current = await _manager.GetOwnedAsync(id);
                    if (current == null) { return NotFound("不存在的资源"); };

            """;
        string entityName = EntityInfo.Name;
        List<Entity.PropertyInfo>? requiredNavigations = EntityInfo.GetRequiredNavigation();

        requiredNavigations?.ForEach(nav =>
        {
            string name = nav.Type;
            string manager = "_" + name.ToCamelCase() + "Manager";
            string variable = name.ToCamelCase();
            if (!name.Equals("User") && !name.Equals("SystemUser"))
            {
                content += $$"""
                        if (dto.{{name}}Id != null && current.{{nav.Name}}.Id != dto.{{name}}Id)
                        {
                            var {{variable}} = await {{manager}}.GetCurrentAsync(dto.{{name}}Id.Value);
                            if ({{variable}} == null) { return NotFound("不存在的{{nav.CommentSummary ?? name}}"); }
                            current.{{nav.Name}} = {{variable}};
                        }

                """;
            }
        });
        content += """
                    return await _manager.UpdateAsync(current, dto);
            """;
        return content;
    }

    /// <summary>
    /// get user DbContext name
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public string GetContextName(string? contextName = null)
    {
        string name = "ContextBase";
        string? assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(ApplicationPath));
        CompilationHelper cpl = new(ApplicationPath, assemblyName);
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
        return tplContent.Replace(TplConst.ADDICTION_MANAGER_PROPS, "")
            .Replace(TplConst.ADDICTION_MANAGER_DI, "");

    }
}
