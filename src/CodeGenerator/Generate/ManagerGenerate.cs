using System.Reflection.Metadata;

namespace CodeGenerator.Generate;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerGenerate : GenerateBase
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
    /// DTO 所在项目目录路径
    /// </summary>
    public string SharePath { get; set; }
    public string? ContextName { get; set; }
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; }
    public string? ServiceNamespace { get; set; }
    public readonly EntityInfo EntityInfo;
    public ManagerGenerate(string entityPath, string dtoPath, string servicePath, string? contextName = null)
    {
        EntityPath = entityPath;
        SharePath = dtoPath;
        StorePath = servicePath;
        ContextName = contextName;
        ShareNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(SharePath));
        ServiceNamespace = AssemblyHelper.GetNamespaceName(new DirectoryInfo(StorePath));
        EntityParseHelper entityHelper = new(entityPath);
        EntityInfo = entityHelper.GetEntity();
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
        var entityHelper = new EntityParseHelper(EntityPath);
        entityHelper.Parse();
        tplContent = tplContent.Replace(TplConst.ENTITY_NAMESPACE, entityHelper.NamespaceName)
            .Replace(TplConst.NAMESPACE, ServiceNamespace);
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
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
        var addDtoPath = Path.Combine(SharePath, "Models", EntityInfo.Name + "Dtos", EntityInfo.Name + "AddDto.cs");
        var entityHelper = new EntityParseHelper(addDtoPath);
        entityHelper.Parse();
        // 构造内容
        string content = $$"""
                    var dto = new {{EntityInfo.Name}}AddDto()
                    {

            """;
        var requiredProps = entityHelper.PropertyInfos?.Where(i => i.IsRequired).ToList();
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
            content += $"{row + Environment.NewLine}".Indent(3);
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
        var addDtoPath = Path.Combine(SharePath, "Models", EntityInfo.Name + "Dtos", EntityInfo.Name + "AddDto.cs");
        var entityHelper = new EntityParseHelper(addDtoPath);
        entityHelper.Parse();
        // 构造内容
        string content = $$"""
                    var dto = new {{EntityInfo.Name}}UpdateDto()
                    {

            """;
        var requiredProps = entityHelper.PropertyInfos?.Where(i => i.IsRequired).ToList();
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
            content += $"{row + Environment.NewLine}".Indent(3);
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
        FileInfo fileInfo = new(EntityPath);
        FileInfo? projectFile = AssemblyHelper.FindProjectFile(fileInfo.Directory!, fileInfo.Directory!.Root);
        string? entityProjectNamespace = AssemblyHelper.GetNamespaceName(projectFile!.Directory!);

        CompilationHelper compilationHelper = new(projectFile.Directory!.FullName);
        string content = File.ReadAllText(fileInfo.FullName, Encoding.UTF8);
        compilationHelper.AddSyntaxTree(content);
        string? entityNamespace = compilationHelper.GetNamesapce();

        return new List<string>
        {
            "global using System;",
            "global using System.Text.Json;",
            "global using EntityFramework;",
            "global using Microsoft.EntityFrameworkCore;",
            "global using Microsoft.Extensions.Logging;",
            $"global using {entityProjectNamespace}.Utils;",
            $"global using {entityProjectNamespace}.Entities;",
            $"global using {entityProjectNamespace}.Models;",
            $"global using {entityNamespace};",
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
        if (queryOrCommand is not "Query" and not "Command")
        {
            throw new ArgumentException("不允许的参数");
        }
        string contextName = queryOrCommand + "DbContext";
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
        // 生成基础仓储实现类，替换模板变量并写入文件
        string tplContent = GetTplContent($"Implement.{queryOrCommand}StoreContent.tpl");
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
        string queryPath = Path.Combine(StorePath, $"{Const.QUERY_STORE}");
        string[] queryFiles = Directory.GetFiles(queryPath, $"*{Const.QUERY_STORE}.cs", SearchOption.TopDirectoryOnly);
        string commandPath = Path.Combine(StorePath, $"{Const.COMMAND_STORE}");
        string[] commandFiles = Directory.GetFiles(commandPath, $"*{Const.COMMAND_STORE}.cs", SearchOption.TopDirectoryOnly);
        IEnumerable<string> allDataStores = queryFiles.Concat(commandFiles);

        string props = "";
        string ctorParams = "";
        string ctorAssign = "";
        string oneTab = "    ";
        string twoTab = "        ";
        string usings = "";
        if (allDataStores.Any())
        {
            var compilationHelper = new CompilationHelper(SharePath);
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

                string row = $"{oneTab}public {propType}<{propGeneric}> {propName} {{ get; init; }}";
                props += row + Environment.NewLine;
                // 构造函数参数
                row = $"{twoTab}{fileName} {propName.ToCamelCase()},";
                ctorParams += row + Environment.NewLine;
                // 构造函数赋值
                row = $"{twoTab}{propName} = {propName.ToCamelCase()};";
                ctorAssign += row + Environment.NewLine;
                ctorAssign += $"{twoTab}AddCache({propName});" + Environment.NewLine;
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
        content = content.Replace(TplConst.NAMESPACE, ServiceNamespace)
            .Replace(TplConst.STORECONTEXT_PROPS, props)
            .Replace(TplConst.STORECONTEXT_PARAMS, ctorParams)
            .Replace(TplConst.STORECONTEXT_ASSIGN, ctorAssign);
        return usings + content;
    }

    /// <summary>
    /// 服务注册代码
    /// </summary>
    /// <returns></returns>
    public string GetStoreService()
    {
        string storeServiceContent = "";
        string managerServiceContent = "";

        // 获取所有data stores
        string storeDir = Path.Combine(StorePath, "DataStore");
        string[] files = Array.Empty<string>();

        if (Directory.Exists(storeDir))
        {
            files = Directory.GetFiles(storeDir, "*DataStore.cs", SearchOption.TopDirectoryOnly);
        }

        string[] queryFiles = Directory.GetFiles(Path.Combine(StorePath, $"{Const.QUERY_STORE}"), $"*{Const.QUERY_STORE}.cs", SearchOption.TopDirectoryOnly);
        string[] commandFiles = Directory.GetFiles(Path.Combine(StorePath, $"{Const.COMMAND_STORE}"), $"*{Const.COMMAND_STORE}.cs", SearchOption.TopDirectoryOnly);

        files = files.Concat(queryFiles).Concat(commandFiles).ToArray();

        files?.ToList().ForEach(file =>
            {
                object name = Path.GetFileNameWithoutExtension(file);
                string row = $"        services.AddScoped(typeof({name}));";
                storeServiceContent += row + Environment.NewLine;
            });

        // 获取所有manager
        string managerDir = Path.Combine(StorePath, "Manager");
        if (!Directory.Exists(managerDir))
        {
            return string.Empty;
        }

        files = Directory.GetFiles(managerDir, "*Manager.cs", SearchOption.TopDirectoryOnly);

        files?.ToList().ForEach(file =>
            {
                object name = Path.GetFileNameWithoutExtension(file);
                string row = $"        services.AddScoped<I{name}, {name}>();";
                managerServiceContent += row + Environment.NewLine;
            });

        // 构建服务
        string content = GetTplContent("Implement.StoreServicesExtensions.tpl");
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
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
        string tplContent = GetTplContent($"Implement.IManager.tpl");
        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.ID_TYPE, Config.IdType)
            .Replace(TplConst.NAMESPACE, ServiceNamespace);
        return tplContent;
    }

    /// <summary>
    /// Manager默认代码内容
    /// </summary>
    /// <returns></returns>
    public string GetManagerContent()
    {
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
        string tplContent = GetTplContent($"Implement.Manager.tpl");

        // 依赖注入
        string additionManagerProps = "";
        string additionManagerDI = "";
        string additionManagerInit = "";

        var navigations = EntityInfo.PropertyInfos.Where(p => p.IsNavigation && p.HasMany == true)
            .ToList();
        navigations?.ForEach(navigation =>
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

        // 方法内容
        tplContent = tplContent.Replace("${AddActionBlock}", GetAddMethodContent())
            .Replace("${UpdateActionBlock}", GetUpdateMethodContent())
            .Replace("${FilterActionBlock}", GetFilterMethodContent());

        tplContent = tplContent.Replace(TplConst.ENTITY_NAME, entityName)
            .Replace(TplConst.ID_TYPE, Config.IdType)
            .Replace(TplConst.NAMESPACE, ServiceNamespace);
        return tplContent;
    }

    public string GetAddMethodContent()
    {
        string entityName = EntityInfo.Name;
        string content = $$"""
                    var entity = dto.MapTo<{{entityName}}AddDto, {{entityName}}>();

            """;
        // 包含的关联内容
        var navigations = EntityInfo.PropertyInfos.Where(p => p.IsNavigation && p.HasMany == true)
          .ToList();
        navigations?.ForEach(nav =>
        {
            var manager = "_" + nav.Type.ToCamelCase() + "Manager";
            var variable = nav.Name.ToCamelCase();
            content += $$"""
                    if (dto.{{nav.Type}}Ids != null && dto.{{nav.Type}}Ids.Any())
                    {
                        var {{variable}}= await {{manager}}.Command.Db.Where(t => dto.{{nav.Type}}Ids.Contains(t.Id)).ToListAsync();
                        if ({{variable}} != null)
                        {
                            entity.{{nav.Name}} = {{variable}};
                        }
                    }
            """;
        });
        // 所属的关联内容
        var requiredNavigations = EntityInfo.GetRequiredNavigation();
        requiredNavigations?.ForEach(nav =>
        {
            var manager = "_" + nav.Type.ToCamelCase() + "Manager";
            var idName = nav.Name + "Id";
            if (nav.Type != "User" && nav.Type != "SystemUser")
            {
                content += $$"""
                        Command.Db.Entry(entity).Property("{{idName}}").CurrentValue = dto.{{idName}};
                        // or entity.{{idName}} = dto.{{idName}};

                """;
            }
            else
            {
                content += $$"""
                        Command.Db.Entry(entity).Property("{{idName}}").CurrentValue = _userContext.UserId!.Value;
                        // or entity.{{idName}} = _userContext.UserId!.Value;

                """;
            }
        });
        content += $$"""      
                // other required props
                return {{(navigations == null ? "entity" : "Task.FromResult(entity)")}};
        """;
        return content;
    }
    public string GetUpdateMethodContent()
    {
        string content = "";
        // 包含的关联内容
        var navigations = EntityInfo.PropertyInfos.Where(p => p.IsNavigation && p.HasMany == true)
          .ToList();
        navigations?.ForEach(nav =>
        {
            var manager = "_" + nav.Type.ToCamelCase() + "Manager";
            var variable = nav.Name.ToCamelCase();
            content += $$"""
                    if (dto.{{nav.Type}}Ids != null && dto.{{nav.Type}}Ids.Any())
                    {
                        var {{variable}}= await {{manager}}.Command.Db.Where(t => dto.{{nav.Type}}Ids.Contains(t.Id)).ToListAsync();
                        if ({{variable}} != null)
                        {
                            entity.{{nav.Name}} = {{variable}};
                        }
                    }
            """;
        });
        content += """      return await base.UpdateAsync(entity, dto);""";
        return content;
    }
    public string GetFilterMethodContent()
    {
        string content = "";
        string entityName = EntityInfo.Name;
        var props = EntityInfo.PropertyInfos.Where(p => !p.IsList)
            .Where(p => p.IsRequired && !p.IsNullable)
            .Where(p => !p.Name.EndsWith("Id"))
            .Where(p => p.MaxLength is not (not null and >= 200))
            .ToList();

        if (props.Any())
        {
            content += """
                    // https://github.com/AterDev/ater.web/blob/56542e5653ee795855705e43482e64df0ee8383d/templates/apistd/src/Core/Utils/Extensions.cs#L82
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
    /// 服务注册
    /// </summary>
    /// <param name="dataStores">all store names</param>
    /// <returns></returns>
    public string GetStoreServiceAfterBuild()
    {
        string storeServiceDIContent = "";
        // 获取所有继承了 DataStoreBase 的类
        string? assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(StorePath));
        CompilationHelper cpl = new(StorePath, assemblyName);
        IEnumerable<INamedTypeSymbol> classes = cpl.AllClass;
        if (classes != null)
        {
            IEnumerable<INamedTypeSymbol> allDataStores = CompilationHelper.GetClassNameByBaseType(classes, "DataStoreBase");
            if (allDataStores.Any())
            {
                allDataStores.ToList().ForEach(dataStore =>
                {
                    string row = $"        services.AddScoped(typeof({dataStore.Name}));";
                    storeServiceDIContent += row + Environment.NewLine;
                });
            }
        }
        // 构建服务
        string content = GetTplContent("Implement.DataStoreExtensioins.tpl");
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
        string name = "ContextBase";
        string? assemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(StorePath));
        CompilationHelper cpl = new(StorePath, assemblyName);
        IEnumerable<INamedTypeSymbol> classes = cpl.AllClass;
        if (classes != null)
        {
            // 获取所有继承 dbcontext的上下文
            IEnumerable<INamedTypeSymbol> allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, baseTypeName: "IdentityDbContext");
            if (!allDbContexts.Any())
            {
                allDbContexts = CompilationHelper.GetClassNameByBaseType(classes, "DbContext");
            }

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
        DirectoryInfo entityDir = new FileInfo(EntityPath).Directory!;
        FileInfo? entityProjectFile = AssemblyHelper.FindProjectFile(entityDir, entityDir.Root);
        string? entityNamespace = AssemblyHelper.GetNamespaceName(entityProjectFile!.Directory!);
        string tplContent = GetTplContent("Extensions.tpl");
        tplContent = tplContent.Replace(TplConst.NAMESPACE, entityNamespace);
        return tplContent;
    }
}
