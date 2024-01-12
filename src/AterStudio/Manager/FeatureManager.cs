using System.Text.Json.Nodes;

using AterStudio.Models;

using Command.Share.Commands;

using Core.Infrastructure.Helper;

namespace AterStudio.Manager;
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
        var projectType = dto.ProjectType == ProjectType.GRPC ? "-g" : "";
        var apiName = dto.ProjectType == ProjectType.GRPC ? "Grpc.API" : "Http.API";

        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(path);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"new atapi -o {path} {projectType}", out _))
        {
            ErrorMsg = "创建项目失败，请尝试使用空目录创建";
            return false;
        }
        await Console.Out.WriteLineAsync($"✅ create new solution {path}");

        // 修改配置文件
        var configFile = Path.Combine(path, "src", apiName, "appsettings.json");
        var jsonString = File.ReadAllText(configFile);
        var jsonNode = JsonNode.Parse(jsonString, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip
        });

        if (jsonNode != null)
        {
            //JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Database", dto.DBType.ToString().ToLower());
            //JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Cache", dto.CacheType.ToString().ToLower());

            if (!string.IsNullOrWhiteSpace(dto.CommandDbConnStrings))
            {
                JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CommandDb", dto.CommandDbConnStrings);
            }
            if (!string.IsNullOrWhiteSpace(dto.QueryDbConnStrings))
            {
                JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.QueryDb", dto.QueryDbConnStrings);
            }
            if (!string.IsNullOrWhiteSpace(dto.CacheConnStrings))
            {
                JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.Cache", dto.CacheConnStrings);
            }
            if (!string.IsNullOrWhiteSpace(dto.CacheInstanceName))
            {
                JsonHelper.AddOrUpdateJsonNode(jsonNode, "ConnectionStrings.CacheInstanceName", dto.CacheInstanceName);
            }

            jsonString = jsonNode.ToString();
            File.WriteAllText(configFile, jsonString);
        }

        // 移除默认的微服务
        var defaultServicePath = Path.Combine(path, "src", "Microservice", "StandaloneService");
        if (Directory.Exists(defaultServicePath))
        {
            // 从解决方案移除项目
            ProcessHelper.RunCommand("dotnet", $"sln {path} remove {Path.Combine(defaultServicePath, "StandaloneService.csproj")}", out string error);
            Directory.Delete(defaultServicePath, true);
        }
        // TODO:前端项目处理
        if (dto.FrontType == FrontType.None)
        {
            var appPath = Path.Combine(path, "src", "ClientApp");
            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }
        }

        // 模块
        if (dto.HasFileManagerFeature)
        {
            await ModuleCommand.CreateModuleAsync(path, ModuleCommand.FileManager);
        }
        else
        {
            ModuleCommand.CleanModule(path, ModuleCommand.FileManager);
        }
        if (dto.HasOrderFeature)
        {
            await ModuleCommand.CreateModuleAsync(path, ModuleCommand.Order);
        }
        else
        {
            ModuleCommand.CleanModule(path, ModuleCommand.Order);
        }

        if (dto.HasCmsFeature)
        {
            await ModuleCommand.CreateModuleAsync(path, ModuleCommand.CMS);
        }
        else
        {
            ModuleCommand.CleanModule(path, ModuleCommand.CMS);
        }

        if (dto.HasSystemFeature)
        {
            await ModuleCommand.CreateModuleAsync(path, ModuleCommand.System);
        }
        else
        {
            ModuleCommand.CleanModule(path, ModuleCommand.System);
        }

        // 添加项目
        var addRes = await _projectManager.AddProjectAsync(dto.Name, path);
        if (addRes != null)
        {
            ErrorMsg = addRes;
            return false;
        }
        return true;
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
            await ModuleCommand.CreateModuleAsync(_projectContext.SolutionPath!, name);
        }
        catch (Exception e)
        {
            ErrorMsg = e.Message;
            return false;
        }
        return true;
    }
}
