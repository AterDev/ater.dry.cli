using System.Text.Json;
using AterStudio.Entity;
using AterStudio.Models;

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
        // TODO:获取并构造参数
        FileInfo slnFile = new(path);
        string? dir = slnFile.DirectoryName;

        string configFilePath = Path.Combine(dir, ".droplet-config.json");
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException("未找到配置文件:.droplet-config.json");
        }

        string configJson = await File.ReadAllTextAsync(Path.Combine(dir, ".droplet-config.json"));
        ConfigOptions? config = JsonSerializer.Deserialize<ConfigOptions>(configJson);

        string projectName = Path.GetFileNameWithoutExtension(path);
        Project project = new()
        {
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
}
