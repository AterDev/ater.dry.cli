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

    public List<Project> GetProjects()
    {
        var projects = _db.Projects.FindAll().ToList();
        projects.ForEach(async p =>
        {
            var configFilePath = Path.Combine(p.Path, "..", Config.ConfigFileName);
            string configJson = await File.ReadAllTextAsync(configFilePath);
            ConfigOptions? config = JsonSerializer.Deserialize<ConfigOptions>(configJson);
            p.Version = config!.Version;
        });

        return projects;
    }

    public async Task<Project?> AddProjectAsync(string name, string path)
    {
        // 获取并构造参数
        FileInfo projectFile = new(path);
        bool hasProjectFile = true;
        // 如果是目录
        if ((projectFile.Attributes & FileAttributes.Directory) != 0)
        {
            var projectFilePath = Directory.GetFiles(path, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (projectFilePath != null)
            {
                projectFile = new FileInfo(projectFilePath);
            }
            else
            {
                projectFilePath = Directory.GetFiles(path, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (projectFilePath != null)
                {
                    projectFile = new FileInfo(projectFilePath);
                }
                else
                {
                    hasProjectFile = false;
                }
            }
        }

        string dir = hasProjectFile ? projectFile.DirectoryName! : projectFile.FullName;
        string configFilePath = Path.Combine(dir!, Config.ConfigFileName);

        await ConfigCommand.InitConfigFileAsync(dir);
        string configJson = await File.ReadAllTextAsync(configFilePath);

        ConfigOptions? config = JsonSerializer.Deserialize<ConfigOptions>(configJson);

        string projectName = Path.GetFileNameWithoutExtension(projectFile.FullName);
        Project project = new()
        {
            ProjectId = config!.ProjectId,
            DisplayName = name,
            Path = projectFile.FullName,
            Name = projectName,
            Version = config.Version
        };

        _db.Projects.EnsureIndex(p => p.ProjectId);
        _db.Projects.Insert(project);

        return project;
    }

    public bool DeleteProject(Guid id)
    {
        return _db.Projects.Delete(id);
    }

    public async Task<Project> GetProjectAsync(Guid id)
    {
        var project = _db.Projects.FindById(id);
        var configFilePath = Path.Combine(project.Path, "..", Config.ConfigFileName);
        string configJson = await File.ReadAllTextAsync(configFilePath);
        ConfigOptions? config = JsonSerializer.Deserialize<ConfigOptions>(configJson);
        project.Version = config!.Version;
        return project;
    }

    public List<SubProjectInfo>? GetAllProjects(Guid id)
    {
        var project = GetProjectAsync(id);
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
            options.StorePath = dto.StorePath;
        if (dto.DtoPath != null)
            options.DtoPath = dto.DtoPath;
        if (dto.ApiPath != null)
            options.ApiPath = dto.ApiPath;
        if (dto.IsSplitController != null)
            options.IsSplitController = dto.IsSplitController;

        string content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(_projectContext.SolutionPath!, Config.ConfigFileName), content, Encoding.UTF8);
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
