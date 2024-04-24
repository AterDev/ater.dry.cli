using System.Text.Json.Nodes;

namespace Application.Manager;
/// <summary>
/// 功能集成
/// </summary>
public class FeatureManager(ProjectContext projectContext, ProjectManager projectManager)
{
    private readonly ProjectContext _projectContext = projectContext;
    private readonly ProjectManager _projectManager = projectManager;

    public string ErrorMsg { get; set; } = string.Empty;

    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CreateNewSolutionAsync(CreateSolutionDto dto)
    {
        // 生成项目
        var path = Path.Combine(dto.Path, dto.Name);
        var apiName = "Http.API";
        var templateType = dto.IsLight ? "atlight" : "atapi";

        var version = AssemblyHelper.GetCurrentToolVersion();

        if (ProcessHelper.RunCommand("dotnet", $"new list atapi", out _))
        {
            ProcessHelper.RunCommand("dotnet", $"new update", out _);
        }
        else
        {
            ProcessHelper.RunCommand("dotnet", $"new install ater.web.templates::{version}", out string msg);
            Console.WriteLine(msg);
        }

        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(path);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"new {templateType} -o {path} --force", out _))
        {
            ErrorMsg = "创建项目失败，请尝试使用空目录创建";
            return false;
        }
        await Console.Out.WriteLineAsync($"✅ create new solution {path}");

        // 更新配置文件
        UpdateAppSettings(dto, path, apiName);
        // 数据库选择
        ChooseDatabase(dto.DBType, path, dto.IsLight);

        // 移除默认的微服务
        var defaultServicePath = Path.Combine(path, "src", "Microservice", "StandaloneService");
        if (Directory.Exists(defaultServicePath))
        {
            // 从解决方案移除项目
            ProcessHelper.RunCommand("dotnet", $"sln {path} remove {Path.Combine(defaultServicePath, "StandaloneService.csproj")}", out string error);
            Directory.Delete(defaultServicePath, true);
        }

        // 前端项目处理
        if (dto.FrontType == FrontType.None)
        {
            var appPath = Path.Combine(path, "src", "ClientApp");
            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }
        }


        // 默认包含SystemMod
        if (!dto.IsLight)
        {
            var moduleCommand = new ModuleCommand(path, ModuleCommand.System);
            await moduleCommand.CreateModuleAsync();

            if (dto.HasFileManagerFeature)
            {
                await moduleCommand.CreateModuleAsync();
            }
            else
            {
                moduleCommand.CleanModule();
            }
            if (dto.HasOrderFeature)
            {
                await moduleCommand.CreateModuleAsync();
            }
            else
            {
                moduleCommand.CleanModule();
            }

            if (dto.HasCmsFeature)
            {
                await moduleCommand.CreateModuleAsync();
            }
            else
            {
                moduleCommand.CleanModule();
            }
        }

        // 保存项目信息
        var addRes = await _projectManager.AddProjectAsync(dto.Name, path);
        if (addRes != null)
        {
            ErrorMsg = addRes;
            return false;
        }

        // restore & build solution
        if (!ProcessHelper.RunCommand("dotnet", $"build {path}", out _))
        {
            ErrorMsg = "项目创建成功，但构建失败，请查看错误信息!";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 更新配置文件
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="path"></param>
    /// <param name="apiName"></param>
    private static void UpdateAppSettings(CreateSolutionDto dto, string path, string apiName)
    {
        // 修改配置文件
        var configFile = Path.Combine(path, "src", apiName, "appsettings.json");
        var jsonString = File.ReadAllText(configFile);
        var jsonNode = JsonNode.Parse(jsonString, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip
        });
        if (jsonNode != null)
        {
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Database", dto.DBType.ToString());
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Cache", dto.CacheType.ToString());
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Key.DefaultPassword", dto.DefaultPassword ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CommandDb", dto.CommandDbConnStrings ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.QueryDb", dto.QueryDbConnStrings ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.Cache", dto.CacheConnStrings ?? "");
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CacheInstanceName", dto.CacheInstanceName ?? "");

            jsonString = jsonNode.ToString();
            File.WriteAllText(configFile, jsonString);
        }
    }

    /// <summary>
    /// 不同数据库处理
    /// </summary>
    /// <param name="dBType"></param>
    /// <param name="path"></param>
    /// <param name="isLight"></param>
    private void ChooseDatabase(DBType dBType, string path, bool isLight)
    {
        var packageNames = new List<string> {
            "Microsoft.EntityFrameworkCore.Sqlite",
            "Microsoft.EntityFrameworkCore.SqlServer",
            "Npgsql.EntityFrameworkCore.PostgreSQL"
        };

        var useMethod = "UseNpgsql";
        var packageName = "Npgsql.EntityFrameworkCore.PostgreSQL";
        if (dBType == DBType.SQLite)
        {
            useMethod = "UseSqlite";
            packageName = "Microsoft.EntityFrameworkCore.Sqlite";
        }
        else if (dBType == DBType.SQLServer)
        {
            useMethod = "UseSqlServer";
            packageName = "Microsoft.EntityFrameworkCore.SqlServer";
        }

        var appServiceFile = Path.Combine(path, "src", "Application", "AppServiceCollectionExtensions.cs");
        var content = File.ReadAllText(appServiceFile);
        content = content.Replace("option.UseNpgsql", "option." + useMethod);
        File.WriteAllText(appServiceFile, content);

        var queryFactoryFile = Path.Combine(path, "src", "Definition", "EntityFramework", "DBProvider", "QueryDbContextFactory.cs");
        content = File.ReadAllText(queryFactoryFile);

        content = content.Replace("builder.UseNpgsql", "builder." + useMethod);
        File.WriteAllText(queryFactoryFile, content);

        var commandFactoryFile = Path.Combine(path, "src", "Definition", "EntityFramework", "DBProvider", "CommandDbContextFactory.cs");
        content = File.ReadAllText(commandFactoryFile);
        content = content.Replace("builder.UseNpgsql", "builder." + useMethod);
        File.WriteAllText(commandFactoryFile, content);

        // 项目引用处理
        var entityProjectFile = Path.Combine(path, "src", "Definition", "Definition.csproj");
        if (!isLight)
        {
            entityProjectFile = Path.Combine(path, "src", "Definition", "EntityFramework", "EntityFramework.csproj");
        }

        packageNames.Remove(packageName);

        // 移除无用包
        var lines = File.ReadAllLines(entityProjectFile).ToList();
        foreach (var item in packageNames)
        {
            var index = lines.FindIndex(x => x.Contains(item));
            if (index > 0)
            {
                lines.RemoveAt(index);
            }
        }
        File.WriteAllLines(entityProjectFile, lines);
    }

    /// <summary>
    /// 获取模块信息
    /// </summary>
    /// <returns></returns>
    public List<SubProjectInfo> GetModulesInfo()
    {
        var res = new List<SubProjectInfo>();
        var paths = ModuleCommand.GetModulesPaths(_projectContext.SolutionPath!);
        paths?.ForEach(path =>
        {
            var moduleInfo = new SubProjectInfo
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Path = path,
                ProjectType = ProjectType.Module
            };
            res.Add(moduleInfo);
        });
        return res;
    }

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="name"></param>
    public async Task<bool> CreateModuleAsync(string name)
    {
        try
        {
            await new ModuleCommand(_projectContext.SolutionPath!, name).CreateModuleAsync();
        }
        catch (Exception e)
        {
            ErrorMsg = e.Message;
            return false;
        }
        return true;
    }
}
