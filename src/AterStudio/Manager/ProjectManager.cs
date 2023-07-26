using System.Text;

using AterStudio.Models;

using Command.Share;
using Command.Share.Commands;

using Core;
using Core.Entities;
using Core.Infrastructure;
using Core.Infrastructure.Helper;

using Datastore;

namespace AterStudio.Manager;

public class ProjectManager
{
    private readonly DbContext _db;
    private readonly ProjectContext _projectContext;

    public ProjectManager(DbContext dbContext, ProjectContext projectContext)
    {
        _db = dbContext;
        _projectContext = projectContext;
    }

    public string GetToolVersion()
    {
        return AssemblyHelper.GetCurrentToolVersion();
    }

    public List<Project> GetProjects()
    {
        var projects = _db.Projects.FindAll().ToList();
        projects.ForEach(async p =>
        {
            var configFilePath = Path.Combine(p.Path, "..", Config.ConfigFileName);
            if (File.Exists(configFilePath))
            {
                string configJson = await File.ReadAllTextAsync(configFilePath);
                ConfigOptions? config = JsonSerializer.Deserialize<ConfigOptions>(configJson);
                if (string.IsNullOrWhiteSpace(p.Version))
                {
                    _db.Projects.Update(p);
                }
                p.Version = config!.Version;
            }
            // 移除不存在的项目
            if (!File.Exists(p.Path))
            {
                _db.Projects.Delete(p.Id);
                projects.Remove(p);
            }
        });
        return projects;
    }

    public async Task<string?> AddProjectAsync(string name, string path)
    {
        // 获取并构造参数
        FileInfo projectFile = new(path);
        var projectFilePath = Directory.GetFiles(path, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();

        projectFilePath ??= Directory.GetFiles(path, $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(path, "package.json", SearchOption.TopDirectoryOnly).FirstOrDefault();

        if (projectFilePath == null)
        {
            return "未找到有效的项目";
        }
        var solutionType = AssemblyHelper.GetSolutionType(projectFilePath);

        string configFilePath = Path.Combine(path!, Config.ConfigFileName);

        await ConfigCommand.InitConfigFileAsync(path, solutionType);
        string configJson = await File.ReadAllTextAsync(configFilePath);

        ConfigOptions? config = JsonSerializer.Deserialize<ConfigOptions>(configJson);

        string projectName = Path.GetFileNameWithoutExtension(projectFilePath);
        Project project = new()
        {
            ProjectId = config!.ProjectId,
            DisplayName = name,
            Path = projectFilePath,
            Name = projectName,
            Version = config.Version,
            SolutionType = solutionType
        };

        _db.Projects.EnsureIndex(p => p.ProjectId);
        _db.Projects.Insert(project);

        return default;
    }

    public bool DeleteProject(Guid id)
    {
        return _db.Projects.Delete(id);
    }

    public async Task<Project> GetProjectAsync(Guid id)
    {
        var project = _db.Projects.FindById(id);
        if (string.IsNullOrWhiteSpace(project.Version))
        {
            var solutionPath = Path.Combine(project.Path, "..");
            var version = await AssemblyHelper.GetSolutionVersionAsync(solutionPath);
            project.Version = version;
        }
        return project;
    }

    /// <summary>
    /// 更新解决方案
    /// </summary>
    /// <returns></returns>
    public async Task<string> UpdateSolutionAsync()
    {
        if (_projectContext.Project == null)
        {
            return "未找到有效的项目";
        }
        try
        {
            var path = _projectContext.Project.Path;
            var version = await AssemblyHelper.GetSolutionVersionAsync(_projectContext.SolutionPath!);
            if (version == null) return "未找到项目配置文件，无法进行更新";
            var updateManager = new UpdateManager(path!, version);
            var isSuccess = await updateManager.UpdateInfrastructureAsync();

            var res = updateManager.GetUpdateNotes(isSuccess);
            if (isSuccess)
            {
                // update version to db
                _projectContext.Project.Version = updateManager.TargetVersion;
                _db.Projects.Update(_projectContext.Project);
            }
            return res;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message + ex.StackTrace);
            return ex.Message;
        }
    }

    public async Task<List<SubProjectInfo>?> GetAllProjectsAsync(Guid id)
    {
        var project = await GetProjectAsync(id);
        var pathString = Path.Combine(project.Path, "../");
        var res = new List<SubProjectInfo>();
        try
        {
            var subProjectFiles = new DirectoryInfo(pathString).GetFiles($"*{Const.CSharpProjectExtention}", SearchOption.AllDirectories).ToList();

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
    }

    /// <summary>
    /// 获取项目配置文件内容
    /// </summary>
    /// <returns></returns>
    public ConfigOptions? GetConfigOptions()
    {
        var options = ConfigCommand.ReadConfigFile(_projectContext.SolutionPath!);
        if (options != null)
        {
            options.RootPath = _projectContext.SolutionPath!;
        }
        return options;
    }

    /// <summary>
    /// 更新配置内容
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateConfigAsync(UpdateConfigOptionsDto dto)
    {
        var options = ConfigCommand.ReadConfigFile(_projectContext.SolutionPath!);
        if (options == null)
        {
            return false;
        }

        if (dto.IdType != null)
            options.IdType = dto.IdType;
        if (dto.EntityPath != null)
            options.EntityPath = dto.EntityPath;
        if (dto.StorePath != null)
            options.ApplicationPath = dto.StorePath;
        if (dto.DtoPath != null)
            options.DtoPath = dto.DtoPath;
        if (dto.ApiPath != null)
            options.ApiPath = dto.ApiPath;
        if (dto.IsSplitController != null)
            options.IsSplitController = dto.IsSplitController;

        string content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(_projectContext.SolutionPath!, Config.ConfigFileName), content, new UTF8Encoding(false));
        return true;
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
        WatcherManager.StartWatcher(project.ProjectId, _projectContext.EntityPath!, _projectContext.SharePath!, _projectContext.ApplicationPath!);
    }
    /// <summary>
    /// 关闭监测
    /// </summary>
    /// <param name="project"></param>
    public void StopWatcher(Project project)
    {
        WatcherManager.StopWatcher(project.ProjectId);
    }

    /// <summary>
    /// 可修改的模板内容
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public List<TemplateFile> GetTemplateFiles(Guid id)
    {
        return new List<TemplateFile>
        {
            new TemplateFile()
            {
                Name = "RequestService.axios.service.tpl",
                DisplayName = "Axios请求基础类",
                ProjectId = id
            },
            new TemplateFile()
            {
                Name = "angular.base.service.tpl",
                DisplayName = "Angular请求基础类",
                ProjectId = id
            },
        };
    }

    /// <summary>
    /// save template content
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public bool SaveTemplate(Guid id, string name, string content)
    {
        var file = _db.TemplateFile.Query()
           .Where(f => f.Name.Equals(name))
           .FirstOrDefault();

        if (file == null)
        {
            file = new TemplateFile
            {
                Name = name,
                ProjectId = Const.PROJECT_ID,
                Content = content
            };
            _db.TemplateFile.EnsureIndex(f => f.Name);
            return _db.TemplateFile.Insert(file).AsBoolean;
        }
        else
        {
            file.Content = content;
            file.ProjectId = Const.PROJECT_ID;
            return _db.TemplateFile.Update(file);
        }
    }

    /// <summary>
    /// get template content
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public TemplateFile GetTemplate(Guid id, string name)
    {
        var project = GetProjectAsync(id);
        // 从库中获取，如果没有，则从模板中读取
        var file = _db.TemplateFile.Query()
            .Where(f => f.Name.Equals(name))
            .FirstOrDefault() ?? new TemplateFile
            {
                Name = name,
                ProjectId = id,
                Content = GenerateBase.GetTplContent(name)
            };
        return file;
    }

}
