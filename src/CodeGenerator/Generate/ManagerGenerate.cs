using Core.Entities;

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
    public string? ServiceNamespace { get; set; }
    public readonly EntityInfo? EntityInfo;
    public ManagerGenerate(string entityFilePath, string dtoPath, string applicationPath)
    {
        EntityFilePath = entityFilePath;
        DtoPath = dtoPath;
        ApplicationPath = applicationPath;
        ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(DtoPath));
        ServiceNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(ApplicationPath));
        if (File.Exists(entityFilePath))
        {
            EntityParseHelper entityHelper = new(entityFilePath);
            EntityInfo = entityHelper.GetEntity();
        }
    }

    /// <summary>
    /// 获取接口模板内容
    /// </summary>
    /// <param name="tplName"></param>
    /// <returns></returns>
    public string GetInterfaceFile(string tplName)
    {
        string content = GetTplContent($"Interface.{tplName}.tpl");
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
        string content = GetTplContent($"Implement.{tplName}.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
        return content;
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
            .Replace(TplConst.NAMESPACE, ServiceNamespace);
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
            var row = (p.Type) switch
            {
                "Guid" => $"{p.Name} = new Guid(\"\"),",
                "string" => $"{p.Name} = \"{p.Name}\" + RandomString,",
                "int" or "double" => $"{p.Name} = 0,",
                "bool" => $"{p.Name} = true,",
                _ => p.IsEnum ? $"{p.Name} = 0," : $"",
            };
            content += string.IsNullOrWhiteSpace(row) ? "" : $"{row + Environment.NewLine}".Indent(3);
            if (!p.Name.EndsWith("Id"))
                assertContent += $"Assert.Equal(entity.{p.Name}, res.{p.Name});{Environment.NewLine}".Indent(2);
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
            var row = (p.Type) switch
            {
                "Guid" => $"{p.Name} = new Guid(\"\"),",
                "string" => $"{p.Name} = \"Update{p.Name}\" + RandomString,",
                "int" or "double" => $"{p.Name} = 0,",
                "bool" => $"{p.Name} = true,",
                _ => p.IsEnum ? $"{p.Name} = 0," : $"",
            };
            content += $"{row + Environment.NewLine}".Indent(3);
            if (!p.Name.EndsWith("Id"))
                assertContent += $"Assert.Equal(entity.{p.Name}, res.{p.Name});{Environment.NewLine}".Indent(3);
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

    public string GetUserContextClass()
    {
        string content = GetTplContent("Implement.UserContext.tpl");
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace);
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
        compilationHelper.AddSyntaxTree(content);
        string? entityNamespace = compilationHelper.GetNamesapce();

        return new List<string>
        {
            $"global using {entityProjectNamespace};",
            $"global using {entityNamespace};",
            ""
    };
    }

    /// <summary>
    /// 生成store实现
    /// </summary>
    /// <param name="queryOrCommand">Query or Command</param>
    /// <returns></returns>
    public string GetStoreContent(string queryOrCommand)
    {
        if (queryOrCommand is not "Query" and not "Command")
        {
            throw new ArgumentException("不允许的参数");
        }
        string contextName = queryOrCommand + "DbContext";
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        // 生成基础仓储实现类，替换模板变量并写入文件
        string tplContent = GetTplContent($"Implement.{queryOrCommand}StoreContent.tpl");
        var entityFrameworkNsp = Config.EntityFrameworkPath.Split(Path.DirectorySeparatorChar).Last();
        tplContent = tplContent.Replace(TplConst.NAMESPACE, entityFrameworkNsp);
        //tplContent = tplContent.Replace(TplConst.SHARE_NAMESPACE, ShareNamespace);
        tplContent = tplContent.Replace(TplConst.DBCONTEXT_NAME, contextName);
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName);
        return tplContent;
    }

    /// <summary>
    /// Manager接口内容
    /// </summary>
    /// <returns></returns>
    public string GetIManagerContent(string? nsp = null)
    {
        nsp ??= ServiceNamespace;
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        string tplContent = GetTplContent($"Implement.IManager.tpl");
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.ID_TYPE, Config.IdType)
            .Replace(TplConst.SHARE_NAMESPACE, ShareNamespace)
            .Replace(TplConst.NAMESPACE, nsp);
        return tplContent;
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContent(string? nsp = null)
    {
        nsp ??= ServiceNamespace;

        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        string tplContent = GetTplContent($"Implement.Manager.tpl");

        // 依赖注入
        string additionManagerProps = "";
        string additionManagerDI = "";
        string additionManagerInit = "";

        var navigations = EntityInfo?.PropertyInfos.Where(p => p.IsNavigation && p.HasMany == true)
            .ToList();
        navigations?.ForEach(navigation =>
        {
        });
        tplContent = tplContent.Replace("${AdditionManagersProps}", additionManagerProps)
            .Replace("${AdditionManagersDI}", additionManagerDI)
            .Replace("${AdditionManagersInit}", additionManagerInit);


        // 方法内容
        tplContent = tplContent.Replace("${AddActionBlock}", GetAddMethodContent())
            .Replace("${UpdateActionBlock}", GetUpdateMethodContent())
            .Replace("${FilterActionBlock}", GetFilterMethodContent());


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
                    if (dto.{{name}}Ids != null && dto.{{name}}Ids.Any())
                    {
                        var {{variable}} = await Stores.CommandSet<{{name}}>().Db
                            .Where(t => dto.{{name}}Ids.Contains(t.Id))
                            .ToListAsync();
                        if ({{variable}} != null)
                        {
                            entity.{{nav.Name}} = {{variable}};
                        }
                    }

            """;
        });
        // 所属的关联内容
        var requiredNavigations = EntityInfo?.GetRequiredNavigation();
        requiredNavigations?.ForEach(nav =>
        {
            var name = nav.NavigationName ?? nav.Type;
            var manager = "_" + name.ToCamelCase() + "Manager";
            var idName = nav.Name + "Id";
            if (name != "User" && name != "SystemUser")
            {
                content += $$"""
                        Command.Db.Entry(entity).Property("{{idName}}").CurrentValue = dto.{{name}}Id;
                        // or entity.{{idName}} = dto.{{name}}Id;

                """;
            }
            else
            {
                content += $$"""
                        Command.Db.Entry(entity).Property("{{idName}}").CurrentValue = _userContext!.UserId!.Value;
                        // or entity.{{idName}} = _userContext!.UserId!.Value;

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
                    if (dto.{{name}}Ids != null && dto.{{name}}Ids.Any())
                    {
                        var {{variable}} = await Stores.CommandSet<{{name}}>().Db
                            .Where(t => dto.{{name}}Ids.Contains(t.Id))
                            .ToListAsync();
                        if ({{variable}} != null)
                        {
                            entity.{{nav.Name}} = {{variable}};
                        }
                    }

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
            .Where(p => p.IsRequired && !p.IsNullable
              || p.IsEnum
              || p.Type.StartsWith("bool") && p.Name != "IsDeleted")
            .Where(p => !p.Name.EndsWith("Id"))
            .Where(p => p.MaxLength is not (not null and >= 200))
            .ToList();

        if (props != null && props.Any())
        {
            content += """
                    Queryable = Queryable

            """;
        }
        var last = props?.LastOrDefault();
        props?.ForEach(p =>
        {
            bool isLast = (p == last);
            var name = p.Name;
            if (p.IsNavigation)
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
    /// store上下文
    /// </summary>
    /// <returns></returns>
    public static string GetDataStoreContext(string path, string nspName)
    {
        string queryPath = Path.Combine(path, $"{Const.QUERY_STORE}");
        string[] queryFiles = Directory.GetFiles(queryPath, $"*{Const.QUERY_STORE}.cs", SearchOption.TopDirectoryOnly);
        string commandPath = Path.Combine(path, $"{Const.COMMAND_STORE}");
        string[] commandFiles = Directory.GetFiles(commandPath, $"*{Const.COMMAND_STORE}.cs", SearchOption.TopDirectoryOnly);
        IEnumerable<string> allDataStores = queryFiles.Concat(commandFiles);

        string ctorParams = "";
        string ctorAssign = "";
        string twoTab = "        ";
        string usings = "";
        if (allDataStores.Any())
        {
            var compilationHelper = new CompilationHelper(path);
            var entityClassNames = new List<string>();

            allDataStores.ToList().ForEach(filePath =>
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                // 属性名
                string propName = fileName.Replace("Store", "");
                // 属性类型
                string propType = fileName.EndsWith($"{Const.QUERY_STORE}") ? "QuerySet" : "CommandSet";
                // 属性泛型
                string propGeneric = fileName.Replace($"{Const.QUERY_STORE}", "")
                    .Replace($"{Const.COMMAND_STORE}", "");

                entityClassNames.Add(propGeneric);

                // 构造函数参数
                string row = $"{twoTab}{fileName} {propName.ToCamelCase()},";
                ctorParams += row + Environment.NewLine;
                // 构造函数赋值

                ctorAssign += $"{twoTab}AddCache({propName.ToCamelCase()});" + Environment.NewLine;
            });
            // 关联模型需要引入的命名空间
            var importNamespaces = compilationHelper.GetNamespaceNames(entityClassNames);
            if (importNamespaces.Any())
            {
                importNamespaces.ForEach(n =>
                {
                    usings += $"using {n};" + Environment.NewLine;
                });
            }
        }
        // 构建服务
        string content = GetTplContent("Implement.DataStoreContext.tpl");
        content = content.Replace(TplConst.NAMESPACE, nspName)
            .Replace(TplConst.STORECONTEXT_PROPS, "")
            .Replace(TplConst.STORECONTEXT_PARAMS, ctorParams)
            .Replace(TplConst.STORECONTEXT_ASSIGN, ctorAssign);
        return usings + content;
    }

    /// <summary>
    /// 生成注入服务
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <param name="nspName"></param>
    /// <returns></returns>
    public static string GetManagerDIExtensions(string solutionPath, string nspName)
    {
        string storeServiceContent = "";
        string managerServiceContent = "";

        var entityFrameworkPath = Path.Combine(solutionPath, Config.EntityFrameworkPath);

        // 获取所有data stores
        string[] files = Array.Empty<string>();

        string[] queryFiles = Directory.GetFiles(Path.Combine(entityFrameworkPath, $"{Const.QUERY_STORE}"), $"*{Const.QUERY_STORE}.cs", SearchOption.TopDirectoryOnly);
        string[] commandFiles = Directory.GetFiles(Path.Combine(entityFrameworkPath, $"{Const.COMMAND_STORE}"), $"*{Const.COMMAND_STORE}.cs", SearchOption.TopDirectoryOnly);

        files = files.Concat(queryFiles).Concat(commandFiles).ToArray();

        files?.ToList().ForEach(file =>
        {
            object name = Path.GetFileNameWithoutExtension(file);
            string row = $"        services.AddScoped(typeof({name}));";
            storeServiceContent += row + Environment.NewLine;
        });

        // 获取所有manager
        var application = Path.Combine(solutionPath, Config.ApplicationPath);
        string managerDir = Path.Combine(application, "Manager");
        if (!Directory.Exists(managerDir))
        {
            return string.Empty;
        }

        files = Directory.GetFiles(managerDir, "*Manager.cs", SearchOption.TopDirectoryOnly);

        files?.ToList().ForEach(file =>
        {
            object name = Path.GetFileNameWithoutExtension(file);
            string row = $"        services.AddScoped(typeof({name}));";
            managerServiceContent += row + Environment.NewLine;
        });

        // 构建服务
        string content = GetTplContent("Implement.ManagerServiceCollectionExtensions.tpl");
        content = content.Replace(TplConst.NAMESPACE, nspName);
        content = content.Replace(TplConst.SERVICE_STORES, storeServiceContent);
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

        string[] files = Array.Empty<string>();
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
