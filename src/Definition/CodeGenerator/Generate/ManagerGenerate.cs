namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerGenerate(EntityInfo entityInfo)
{
    public string ApplicationNamespace { get; init; } = entityInfo.GetManagerNamespace();
    public string ShareNamespace { get; init; } = entityInfo.GetShareNamespace();
    public EntityInfo EntityInfo { get; init; } = entityInfo;

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
            $"global using {ApplicationNamespace}.{Const.ManagersDir};",
            ""
        ];
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContent(string tplContent, string nsp)
    {
        var genContext = new GenContext();
        var model = new ManagerViewModel
        {
            Namespace = nsp,
            EntityName = EntityInfo.Name,
            ShareNamespace = ShareNamespace,
            Comment = EntityInfo.Comment,
            FilterCode = GetFilterMethodContent()
        };

        return genContext.GenManager(tplContent, model);
    }

    private string GetFilterMethodContent()
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
    /// Manager服务注入内容
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <returns></returns>
    public static string GetManagerServiceContent(EntityInfo entityInfo)
    {
        var managerPath = entityInfo.GetManagerPath();
        var nspName = entityInfo.GetManagerNamespace();

        if (!Directory.Exists(managerPath))
        {
            return string.Empty;
        }
        string managerServiceContent = "";
        var files = Directory.GetFiles(managerPath, "*Manager.cs", SearchOption.TopDirectoryOnly);
        files?.ToList().ForEach(file =>
        {
            object name = Path.GetFileNameWithoutExtension(file);
            string row = $"        services.AddScoped(typeof({name}));";
            managerServiceContent += row + Environment.NewLine;
        });

        var genContext = new GenContext();
        var managerModel = new ManagerServiceViewModel
        {
            Namespace = nspName,
            ManagerServices = managerServiceContent
        };
        var tplContent = TplContent.GetManagerServiceExtensionTpl(entityInfo.ModuleName.NotEmpty());
        return genContext.GenCode(tplContent, managerModel);
    }
}