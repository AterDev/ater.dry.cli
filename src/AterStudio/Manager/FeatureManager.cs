using System.Text.Json.Nodes;
using AterStudio.Models;
using Command.Share.Commands;
using Core.Infrastructure.Helper;

namespace AterStudio.Manager;
/// <summary>
/// 功能集成
/// </summary>
public class FeatureManager
{
    private readonly ProjectContext _projectContext;
    private readonly ProjectManager _projectManager;

    public string ErrorMsg { get; set; } = string.Empty;


    public FeatureManager(ProjectContext projectContext, ProjectManager projectManager)
    {
        _projectContext = projectContext;
        _projectManager = projectManager;
    }

    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CreateNewSolutionAsync(CreateSolutionDto dto)
    {
        // 生成项目
        var path = Path.Combine(dto.Path, dto.Name);
        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(path);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"new atapi.pro -o {path}", out _))
        {
            ErrorMsg = "创建项目失败，请尝试使用空目录创建";
            return false;
        }
        await Console.Out.WriteLineAsync($"✅ create new solution {path}");

        // 修改配置文件
        var configFile = Path.Combine(path, "src", "Http.API", "appsettings.json");
        var jsonString = File.ReadAllText(configFile);
        var jsonNode = JsonNode.Parse(jsonString, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip
        });

        if (jsonNode != null)
        {
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Database", dto.DBType.ToString().ToLower());
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Cache", dto.CacheType.ToString().ToLower());

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
            await Console.Out.WriteLineAsync(jsonString);
            File.WriteAllText(configFile, jsonString);
        }

        // 添加项目
        //await _projectManager.AddProjectAsync(dto.Name, dto.Path);

        return true;
    }

    /// <summary>
    /// 获取模块信息
    /// </summary>
    /// <returns></returns>
    public List<SubProjectInfo> GetModulesInfo()
    {
        var res = new List<SubProjectInfo>();
        var paths = ModuleCommand.GetModulesPaths(_projectContext.ProjectPath!);
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
            await ModuleCommand.CreateModuleAsync(_projectContext.ProjectPath!, name);
        }
        catch (Exception e)
        {
            ErrorMsg = e.Message;
            return false;
        }
        return true;
    }

    public void CreateMicroService()
    {

    }
}
