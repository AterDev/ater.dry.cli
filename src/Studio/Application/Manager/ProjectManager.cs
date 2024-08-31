using System.Text;

using CodeGenerator.Helper;

namespace Application.Manager;

public class ProjectManager(CommandDbContext dbContext, ProjectContext projectContext)
{
    private readonly CommandDbContext _db = dbContext;
    private readonly ProjectContext _projectContext = projectContext;
    public string GetToolVersion()
    {
        return AssemblyHelper.GetCurrentToolVersion();
    }
    public async Task<List<Project>> GetProjectsAsync()
    {
        List<Project> projects = _db.Projects.ToList();

        for (int i = 0; i < projects.Count; i++)
        {
            Project p = projects[i];
            string configFilePath = Path.Combine(p.Path, "..", Config.ConfigFileName);
            if (File.Exists(configFilePath))
            {
                string configJson = await File.ReadAllTextAsync(configFilePath);
                ConfigOptions? config = ConfigOptions.ParseJson(configJson);
                if (string.IsNullOrWhiteSpace(p.Version))
                {
                    _db.Projects.Update(p);
                }
                p.Version = config!.Version;
            }
            // 移除不存在的项目
            if (!File.Exists(p.Path))
            {
                _db.Projects.Remove(p);
                _db.SaveChanges();
                projects.Remove(p);
            }
        }
        return projects;
    }

    public async Task<string?> AddProjectAsync(string name, string path)
    {
        // 获取并构造参数
        FileInfo projectFile = new(path);
        string? projectFilePath = Directory.GetFiles(path, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();

        projectFilePath ??= Directory.GetFiles(path, $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(path, "package.json", SearchOption.TopDirectoryOnly).FirstOrDefault();

        if (projectFilePath == null)
        {
            return "未找到有效的项目";
        }
        SolutionType? solutionType = AssemblyHelper.GetSolutionType(projectFilePath);

        string configFilePath = Path.Combine(path!, Config.ConfigFileName);

        await ConfigCommand.InitConfigFileAsync(path, solutionType);
        string configJson = await File.ReadAllTextAsync(configFilePath);

        var config = ConfigOptions.ParseJson(configJson);

        string projectName = Path.GetFileNameWithoutExtension(projectFilePath);
        Project project = new()
        {
            DisplayName = name,
            Path = projectFilePath,
            Name = projectName,
            Version = config!.Version,
            SolutionType = solutionType
        };

        _db.Add(project);
        await _db.SaveChangesAsync();

        return default;
    }

    public bool DeleteProject(Guid id)
    {
        Project? project = _db.Projects.Find(id);
        _db.Projects.Remove(project);
        return _db.SaveChanges() > 0;
    }

    public async Task<Project> GetProjectAsync(Guid id)
    {
        Project? project = _db.Projects.Find(id);
        if (string.IsNullOrWhiteSpace(project.Version))
        {
            string solutionPath = Path.Combine(project.Path, "..");
            string? version = await AssemblyHelper.GetSolutionVersionAsync(solutionPath);
            project.Version = version;
        }
        return project;
    }

    /// <summary>
    /// 打开项目
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string OpenSolution(string path)
    {
        var res = ProcessHelper.ExecuteCommands($"start {path}");
        return res;
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
            string path = _projectContext.Project.Path;
            string? version = await AssemblyHelper.GetSolutionVersionAsync(_projectContext.SolutionPath!);
            return version == null ? "未找到项目配置文件，无法进行更新" : "暂不支持该功能";
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message + ex.StackTrace);
            return ex.Message;
        }
    }

    public async Task<List<SubProjectInfo>?> GetAllProjectsAsync(Guid id)
    {
        Project project = await GetProjectAsync(id);
        string pathString = Path.Combine(project.Path, "../");
        List<SubProjectInfo> res = [];
        try
        {
            List<FileInfo> subProjectFiles = new DirectoryInfo(pathString)
                .GetFiles($"*{Const.CSharpProjectExtention}", SearchOption.AllDirectories)
                .ToList();

            if (subProjectFiles.Count != 0)
            {
                subProjectFiles.ForEach(f =>
                {
                    // 判断类型
                    string? type = AssemblyHelper.GetProjectType(f);
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
        ConfigOptions? options = ConfigCommand.ReadConfigFile(_projectContext.SolutionPath!);
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
        ConfigOptions? options = ConfigCommand.ReadConfigFile(_projectContext.SolutionPath!);
        if (options == null)
        {
            return false;
        }

        if (dto.IdType != null)
        {
            options.IdType = dto.IdType;
        }

        if (dto.EntityPath != null)
        {
            options.EntityPath = dto.EntityPath;
        }

        if (dto.EntityFrameworkPath != null)
        {
            options.DbContextPath = dto.EntityFrameworkPath;
        }

        if (dto.StorePath != null)
        {
            options.ApplicationPath = dto.StorePath;
        }

        if (dto.DtoPath != null)
        {
            options.DtoPath = dto.DtoPath;
        }

        if (dto.ApiPath != null)
        {
            options.ApiPath = dto.ApiPath;
        }

        if (dto.IsSplitController != null)
        {
            options.IsSplitController = dto.IsSplitController;
        }

        if (dto.ControllerType != null)
        {
            options.ControllerType = dto.ControllerType.Value;
        }

        string content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Path.Combine(_projectContext.SolutionPath!, Config.ConfigFileName), content, new UTF8Encoding(false));
        return true;
    }

    /// <summary>
    /// 可修改的模板内容
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public List<TemplateFile> GetTemplateFiles(Guid id)
    {
        return
        [
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
        ];
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
        var file = _db.TemplateFiles
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
            _db.TemplateFiles.Add(file);
        }
        else
        {
            file.Content = content;
            file.ProjectId = Const.PROJECT_ID;
            _db.TemplateFiles.Update(file);
        }
        return _db.SaveChanges() > 0;
    }

    /// <summary>
    /// get template content
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public TemplateFile GetTemplate(Guid id, string name)
    {
        Task<Project> project = GetProjectAsync(id);
        // 从库中获取，如果没有，则从模板中读取
        var file = _db.TemplateFiles
            .Where(f => f.Name.Equals(name))
            .FirstOrDefault() ?? new TemplateFile
            {
                Name = name,
                ProjectId = id,
                Content = GenerateBase.GetTplContent(name)
            };
        return file;
    }

    /// <summary>
    /// 添加微服务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool AddServiceProject(string name)
    {
        try
        {
            ProjectCommand.CreateService(_projectContext.SolutionPath, name);
            return true;
        }
        catch (Exception)
        {
            Console.WriteLine("创建服务失败");
            return false;
        }
    }
}
