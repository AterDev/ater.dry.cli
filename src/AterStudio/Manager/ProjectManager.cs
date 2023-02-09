using AterStudio.Models;
using Command.Share;
using Command.Share.Commands;
using Core;
using Core.Infrastructure.Helper;
using Datastore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AterStudio.Manager;

public class ProjectManager
{
    private readonly DbContext _dbContext;

    public ProjectManager(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Project> GetProjects()
    {
        return _dbContext.Projects.FindAll().ToList();
    }

    public async Task<Project?> AddProjectAsync(string name, string path)
    {
        // 获取并构造参数
        FileInfo slnFile = new(path);
        string dir = slnFile.DirectoryName!;
        string configFilePath = Path.Combine(dir!, Config.ConfigFileName);


        await ConfigCommand.InitConfigFileAsync(dir);
        string configJson = await File.ReadAllTextAsync(configFilePath);

        ConfigOptions? config = JsonSerializer.Deserialize<ConfigOptions>(configJson);

        string projectName = Path.GetFileNameWithoutExtension(path);
        Project project = new()
        {
            ProjectId = config!.ProjectId,
            DisplayName = name,
            Path = path,
            Name = projectName,
            ApplicationPath = config.StorePath.ToFullPath("src", dir),
            EntityFrameworkPath = config.DbContextPath.ToFullPath("src", dir),
            EntityPath = config.EntityPath.ToFullPath("src", dir),
            HttpPath = config.ApiPath.ToFullPath("src", dir),
            SharePath = config.DtoPath.ToFullPath("src", dir),
            SwaggerPath = config.SwaggerPath,
            WebAppPath = config.WebAppPath
        };

        _dbContext.Projects.EnsureIndex(p => p.ProjectId);
        _dbContext.Projects.Insert(project);

        return project;
    }

    public Project GetProject(Guid id)
    {
        return _dbContext.Projects.FindById(id);
    }

    public List<SubProjectInfo>? GetAllProjects(Guid id)
    {
        var project = GetProject(id);
        var pathString = Path.Combine(project.Path, "../");
        var res = new List<SubProjectInfo>();
        try
        {
            var subProjectFiles = new DirectoryInfo(pathString).GetFiles("*.csproj", SearchOption.AllDirectories).ToList();

            if (subProjectFiles.Any())
            {
                subProjectFiles.ForEach(f =>
                {
                    // 判断类型
                    var type = AssemblyHelper.GetProjectType(f);
                    res.Add(new SubProjectInfo
                    {
                        Name = f.Name,
                        Path = f.FullName,
                        ProjectType = type switch
                        {
                            "web" => ProjectType.Web,
                            "console" => ProjectType.Console,
                            _ => ProjectType.Lib
                        }
                    });
                });
            }
            return res;
        }
        catch (Exception)
        {
            return default;
        }
        return default;
    }

    /// <summary>
    /// 是否在监视中
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public bool GetWatcherStatus(Project project)
    {
        return WatcherManager.WatcherList.TryGetValue(project.ProjectId, out _);
    }

    /// <summary>
    /// 开启监控
    /// </summary>
    /// <param name="project"></param>
    public void StartWatcher(Project project)
    {
        WatcherManager.StartWatcher(project.ProjectId, project.EntityPath, project.SharePath, project.ApplicationPath);
    }
    /// <summary>
    /// 关闭监测
    /// </summary>
    /// <param name="project"></param>
    public void StopWatcher(Project project)
    {
        WatcherManager.StopWatcher(project.ProjectId);
    }
}
