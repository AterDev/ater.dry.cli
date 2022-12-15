using Command.Share;
using Command.Share.Commands;
using Core;
using Core.Infrastructure;
using Datastore;

namespace AterStudio.Manager;

public class ProjectManager
{
    private readonly ContextBase _context;

    public ProjectManager(ContextBase context)
    {
        _context = context;
    }

    public async Task<Project?> AddProjectAsync(string name, string path)
    {
        // 获取并构造参数
        FileInfo slnFile = new(path);
        string dir = slnFile.DirectoryName!;
        string configFilePath = Path.Combine(dir!, Config.ConfigFileName);

        if (!File.Exists(configFilePath))
        {
            Console.WriteLine("未找到配置文件，初始化配置文件");
            await ConfigCommand.InitConfigFileAsync(dir);
            //throw new FileNotFoundException("未找到配置文件:.droplet-config.json");
        }

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
            SharePath = config.DtoPath.ToFullPath("src", dir)
        };

        _ = await _context.AddAsync(project);
        _ = await _context.SaveChangesAsync();
        return project;
    }


    /// <summary>
    /// 开启监控
    /// </summary>
    /// <param name="project"></param>
    public void StartWatcher(Project project)
    {
        WatcherManager.StartWatcher(project.Id.ToString(), project.EntityPath, project.SharePath, project.ApplicationPath);
    }


    public void StopWatcher(Project project)
    {
        WatcherManager.StopWatcher(project.Id.ToString());
    }
}
