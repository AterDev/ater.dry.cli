namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerGenerate : GenerateBase
{
    /// <summary>
    /// Application 项目目录路径
    /// </summary>
    public string ApplicationPath { get; init; }
    /// <summary>
    /// DTO 所在项目目录路径
    /// </summary>
    public string DtoPath { get; init; }

    public string? ApplicationNamespace { get; init; }
    public string? ShareNamespace { get; init; }
    public EntityInfo EntityInfo { get; init; }
    public ManagerGenerate(EntityInfo entityInfo, string dtoPath, string applicationPath)
    {
        EntityInfo = entityInfo;
        DtoPath = dtoPath;
        ApplicationPath = applicationPath;
        ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(DtoPath));
        ApplicationNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ApplicationPath));
    }

    /// <summary>
    /// 全局依赖
    /// </summary>
    /// <returns></returns>
    public List<string> GetGlobalUsings()
    {
        return
        [
            $"global using {EntityInfo.AssemblyName};",
            $"global using {EntityInfo.NamespaceName};",
            $"global using {ApplicationNamespace}.Manager;",
            ""
        ];
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContent(string tplContent, string? nsp = null)
    {
        var genContext = new GenContext();
        var model = new ManagerViewModel
        {
            Namespace = nsp,
            EntityName = EntityInfo.Name,
            ShareNamespace = ShareNamespace,
            EntityInfo = EntityInfo,
            FilterCode = GetFilterMethodContent()
        };

        return genContext.GenManager(tplContent, model);
    }

    public string GetFilterMethodContent()
    {
        string content = "";
        string entityName = EntityInfo?.Name ?? "";
        List<Entity.PropertyInfo>? props = EntityInfo?.GetFilterProperties();
        if (props != null && props.Count != 0)
        {
            content += """
                    Queryable = Queryable

            """;
        }
        Entity.PropertyInfo? last = props?.LastOrDefault();
        props?.ForEach(p =>
        {
            bool isLast = p == last;
            string name = p.Name;
            if (p.IsNavigation && !p.IsComplexType)
            {
                content += $$"""
                            .WhereNotNull(filter.{{name}}Id, q => q.{{name}}.Id == filter.{{name}}Id){{(isLast ? ";" : "")}}

                """;
            }
            else
            {
                content += $$"""
                            .WhereNotNull(filter.{{name}}, q => q.{{name}} == filter.{{name}}){{(isLast ? ";" : "")}}

                """;
            }
        });
        content += $$"""
                    
                    return await ToPageAsync<{{entityName + Const.FilterDto}},{{entityName + Const.ItemDto}}>(filter);
            """;
        return content;
    }

    /// <summary>
    /// 生成注入服务
    /// </summary>
    /// <param name="managerPath"></param>
    /// <param name="nspName"></param>
    /// <returns></returns>
    public static string GetManagerDIExtensions(string managerPath, string nspName)
    {
        string managerServiceContent = "";
        // 获取所有manager
        if (!Directory.Exists(managerPath))
        {
            return string.Empty;
        }

        string[] files = Directory.GetFiles(Path.Combine(managerPath, "Manager"), "*Manager.cs", SearchOption.TopDirectoryOnly);

        files?.ToList().ForEach(file =>
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string row = $"        services.AddScoped(typeof({name}));";
            managerServiceContent += row + Environment.NewLine;
        });

        // 构建服务
        string content = GetTplContent("Implement.ManagerServiceCollectionExtensions.tpl");
        content = content.Replace(TplConst.NAMESPACE, nspName);
        content = content.Replace(TplConst.SERVICE_MANAGER, managerServiceContent);
        return content;
    }

    /// <summary>
    /// 生成模块的注入服务
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <param name="ModuleName">模块名称</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string GetManagerModuleDIExtensions(string solutionPath, string ModuleName)
    {
        // 获取所有manager
        string modulePath = Path.Combine(solutionPath, "src", "Modules", ModuleName);
        string managerDir = Path.Combine(modulePath, "Manager");
        if (!Directory.Exists(managerDir))
        {
            return string.Empty;
        }

        string[] files = [];
        string managerServiceContent = "";

        files = Directory.GetFiles(managerDir, "*Manager.cs", SearchOption.TopDirectoryOnly);
        files?.ToList().ForEach(file =>
        {
            object name = Path.GetFileNameWithoutExtension(file);
            string row = $"        services.AddScoped(typeof({name}));";
            managerServiceContent += row + Environment.NewLine;
        });

        // 构建服务
        string content = GetTplContent("Implement.ModuleManagerServiceCollectionExtensions.tpl");
        content = content.Replace(TplConst.NAMESPACE, ModuleName);
        content = content.Replace(TplConst.SERVICE_MANAGER, managerServiceContent);
        return content;
    }
}
