namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerGenerate : GenerateBase
{
    /// <summary>
    /// Entity 文件路径
    /// </summary>
    public string EntityFilePath { get; set; }

    public string ApplicationPath { get; set; }
    /// <summary>
    /// DTO 所在项目目录路径
    /// </summary>
    public string DtoPath { get; set; }
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; }
    public string? ApplicationNamespace { get; set; }
    public readonly EntityInfo? EntityInfo;
    public ManagerGenerate(string entityFilePath, string dtoPath, string applicationPath)
    {
        EntityFilePath = entityFilePath;
        DtoPath = dtoPath;
        ApplicationPath = applicationPath;

        if (Config.IsMicroservice)
        {
            ShareNamespace = Config.ServiceName + ".Definition.Share";
            ApplicationNamespace = Config.ServiceName + ".Application";
        }
        else
        {
            ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(DtoPath));
            ApplicationNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ApplicationPath));
        }

        if (File.Exists(entityFilePath))
        {
            EntityParseHelper entityHelper = new(entityFilePath);
            EntityInfo = entityHelper.GetEntity();
        }
    }

    /// <summary>
    /// manager测试内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerTestContent()
    {
        string tplContent = GetTplContent($"Implement.ManagerTest.tpl");
        var entityHelper = new EntityParseHelper(EntityFilePath);
        entityHelper.Parse();
        tplContent = tplContent.Replace(TplConst.ENTITY_NAMESPACE, entityHelper.NamespaceName)
            .Replace(TplConst.NAMESPACE, ApplicationNamespace);
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName);

        var addContent = GenManagerAddTest();
        var updateContent = GenManagerUpdateTest();
        tplContent = tplContent.Replace("${AddContent}", addContent)
            .Replace("${UpdateContent}", updateContent);
        return tplContent;
    }

    private string GenManagerAddTest()
    {
        // 解析add dto
        var addDtoPath = Path.Combine(DtoPath, "Models", EntityInfo?.Name + "Dtos", EntityInfo?.Name + "AddDto.cs");
        var entityHelper = new EntityParseHelper(addDtoPath);
        entityHelper.Parse();
        // 构造内容
        string content = $$"""
                    var dto = new {{EntityInfo?.Name}}AddDto()
                    {

            """;
        var requiredProps = entityHelper.PropertyInfos?.Where(i => i.IsRequired || !i.IsNullable)
            .Where(i => !i.Name.ToLower().Contains("password"))
            .Where(i => !i.IsNavigation && !i.IsList)
            .ToList();
        var assertContent = "";
        requiredProps?.ForEach(p =>
        {
            var row = p.Type switch
            {
                "Guid" => $"{p.Name} = new Guid(\"\"),",
                "string" => $"{p.Name} = \"{p.Name}\" + RandomString,",
                "int" or "double" => $"{p.Name} = 0,",
                "bool" => $"{p.Name} = true,",
                _ => p.IsEnum ? $"{p.Name} = 0," : $"",
            };
            content += string.IsNullOrWhiteSpace(row) ? "" : $"{row + Environment.NewLine}".Indent(3);
            if (!p.Name.EndsWith("Id"))
            {
                assertContent += $"Assert.Equal(entity.{p.Name}, res.{p.Name});{Environment.NewLine}".Indent(2);
            }
        });

        content += $$"""
                    };
                    var entity = await manager.CreateNewEntityAsync(dto);
                    var res = await manager.AddAsync(entity);
            {{assertContent}}
            """;
        return content;
    }
    private string GenManagerUpdateTest()
    {
        // 解析add dto
        var addDtoPath = Path.Combine(DtoPath, "Models", EntityInfo?.Name + "Dtos", EntityInfo?.Name + "AddDto.cs");
        var entityHelper = new EntityParseHelper(addDtoPath);
        entityHelper.Parse();
        // 构造内容
        string content = $$"""
                    var dto = new {{EntityInfo?.Name}}UpdateDto()
                    {

            """;
        var requiredProps = entityHelper.PropertyInfos?.Where(i => i.IsRequired || !i.IsNullable)
            .Where(i => !i.Name.ToLower().Contains("password"))
            .Where(i => !i.IsNavigation && !i.IsList)
            .ToList();
        var assertContent = "";
        requiredProps?.ForEach(p =>
        {
            var row = p.Type switch
            {
                "Guid" => $"{p.Name} = new Guid(\"\"),",
                "string" => $"{p.Name} = \"Update{p.Name}\" + RandomString,",
                "int" or "double" => $"{p.Name} = 0,",
                "bool" => $"{p.Name} = true,",
                _ => p.IsEnum ? $"{p.Name} = 0," : $"",
            };
            content += $"{row + Environment.NewLine}".Indent(3);
            if (!p.Name.EndsWith("Id"))
            {
                assertContent += $"Assert.Equal(entity.{p.Name}, res.{p.Name});{Environment.NewLine}".Indent(3);
            }
        });

        content += $$"""
                    };
                    var entity = await manager.Command.Db.FirstOrDefaultAsync();
                    if (entity != null)
                    {
                        var res = await manager.UpdateAsync(entity, dto);
            {{assertContent}}
                    }
            """;
        return content;
    }

    /// <summary>
    /// 全局依赖
    /// </summary>
    /// <returns></returns>
    public List<string> GetGlobalUsings()
    {
        FileInfo fileInfo = new(EntityFilePath);
        FileInfo? projectFile = AssemblyHelper.FindProjectFile(fileInfo.Directory!, fileInfo.Directory!.Root);
        string? entityProjectNamespace = AssemblyHelper.GetNamespaceName(projectFile!.Directory!);

        CompilationHelper compilationHelper = new(projectFile.Directory!.FullName);
        string content = File.ReadAllText(fileInfo.FullName, new UTF8Encoding(false));
        compilationHelper.LoadContent(content);
        string? entityNamespace = compilationHelper.GetNamespace();

        return
        [
            $"global using {entityProjectNamespace};",
            $"global using {entityNamespace};",
            $"global using {ApplicationNamespace}.Manager;",
            ""
        ];
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContent(string? nsp = null)
    {
        nsp ??= ApplicationNamespace;
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        string tplContent = GetTplContent($"Implement.Manager.tpl");

        // 方法内容
        tplContent = tplContent.Replace(TplConst.ADD_ACTION_BLOCK, GetAddMethodContent())
            .Replace(TplConst.UPDATE_ACTION_BLOCK, GetUpdateMethodContent())
            .Replace(TplConst.FILTER_ACTION_BLOCK, GetFilterMethodContent());

        Console.WriteLine(ShareNamespace);

        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.ID_TYPE, Config.IdType)
            .Replace(TplConst.COMMENT, EntityInfo?.Comment)
            .Replace(TplConst.SHARE_NAMESPACE, ShareNamespace)
            .Replace(TplConst.NAMESPACE, nsp);
        return tplContent;
    }

    public string GetAddMethodContent()
    {
        string entityName = EntityInfo?.Name ?? "";
        string content = $$"""
                    var entity = dto.MapTo<{{entityName}}AddDto, {{entityName}}>();

            """;
        // 包含的关联内容
        var navigations = EntityInfo?.PropertyInfos.Where(p => p.IsNavigation && p.HasMany == true)
          .ToList();
        navigations?.ForEach(nav =>
        {
            var name = nav.NavigationName ?? nav.Type;
            var variable = nav.Name.ToCamelCase();
            content += $$"""
                    /*
                    if (dto.{{name}}Ids != null && dto.{{name}}Ids.Count > 0)
                    {
                        var {{variable}} = await CommandContext.{{nav.Name}}()
                            .Where(t => dto.{{name}}Ids.Contains(t.Id))
                            .ToListAsync();
                        if ({{variable}} != null)
                        {
                            entity.{{nav.Name}} = {{variable}};
                        }
                    }
                    */

            """;
        });
        // 所属的关联内容
        var requiredNavigations = EntityInfo?.GetRequiredNavigation();
        requiredNavigations?.ForEach(nav =>
        {
            var name = nav.NavigationName ?? nav.Type;
            var manager = "_" + name.ToCamelCase() + "Manager";
            var idName = nav.Name + "Id";
            if (name is not "User" and not "SystemUser")
            {
                content += $$"""
                        Command.Db.Entry(entity).Property("{{idName}}").CurrentValue = dto.{{name}}Id;
                        // or entity.{{idName}} = dto.{{name}}Id;

                """;
            }
            else
            {
                content += $$"""
                        Command.Db.Entry(entity).Property("{{idName}}").CurrentValue = _userContext.UserId;
                        // or entity.{{idName}} = _userContext.UserId;

                """;
            }
        });
        content += $$"""      
                // other required props
                return await Task.FromResult(entity);
        """;
        return content;
    }
    public string GetUpdateMethodContent()
    {
        string content = "";
        // 包含的关联内容
        var navigations = EntityInfo?.PropertyInfos.Where(p => p.IsNavigation && p.HasMany == true)
          .ToList();
        navigations?.ForEach(nav =>
        {
            var name = nav.NavigationName ?? nav.Type;
            var variable = nav.Name.ToCamelCase();
            content += $$"""
                    /*
                    if (dto.{{name}}Ids != null && dto.{{name}}Ids.Count > 0)
                    {
                        var {{variable}} = await CommandContext.{{nav.Name}}()
                            .Where(t => dto.{{name}}Ids.Contains(t.Id))
                            .ToListAsync();
                        if ({{variable}} != null)
                        {
                            entity.{{nav.Name}} = {{variable}};
                        }
                    }
                    */

            """;
        });
        content += "return await base.UpdateAsync(entity, dto);".Indent(2);
        return content;
    }
    public string GetFilterMethodContent()
    {
        string content = "";
        string entityName = EntityInfo?.Name ?? "";
        var props = EntityInfo?.PropertyInfos.Where(p => !p.IsList)
            .Where(p => (p.IsRequired && !p.IsNullable)
              || p.IsEnum
              || (p.Type.StartsWith("bool") && p.Name != "IsDeleted"))
            .Where(p => !p.Name.EndsWith("Id"))
            .Where(p => p.MaxLength is not (not null and >= 200))
            .ToList();

        if (props != null && props.Count != 0)
        {
            content += """
                    Queryable = Queryable

            """;
        }
        var last = props?.LastOrDefault();
        props?.ForEach(p =>
        {
            bool isLast = p == last;
            var name = p.Name;
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
                    // TODO: custom filter conditions
                    return await Query.FilterAsync<{{entityName}}ItemDto>(Queryable, filter.PageIndex, filter.PageSize, filter.OrderBy);
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
            IEnumerable<INamedTypeSymbol> allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, baseTypeName: "IdentityDbContext");
            if (!allDbContexts.Any())
            {
                allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "DbContext");
            }

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

        var files = Directory.GetFiles(Path.Combine(managerPath, "Manager"), "*Manager.cs", SearchOption.TopDirectoryOnly);

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
        var modulePath = Path.Combine(solutionPath, "src", "Modules", ModuleName);
        string managerDir = Path.Combine(modulePath, "Manager");
        if (!Directory.Exists(managerDir))
        {
            return string.Empty;
        }

        string[] files = [];
        var managerServiceContent = "";

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
