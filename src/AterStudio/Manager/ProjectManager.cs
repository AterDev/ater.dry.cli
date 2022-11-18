using System.Runtime.CompilerServices;
using System.Text.Json;
using AterStudio.Entity;
using AterStudio.Models;

namespace AterStudio.Manager;


public class ProjectManager
{
    private ContextBase _context;

    public ProjectManager(ContextBase context)
    {
        _context = context;
    }

    public async Task<Project?> AddProjectAsync(string name, string path)
    {
        // TODO:获取并构造参数
        var slnFile = new FileInfo(path);
        var dir = slnFile.DirectoryName;

        var configFilePath = Path.Combine(dir, ".droplet-config.json");
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException("未找到配置文件:.droplet-config.json");
        }

        var configJson = await File.ReadAllTextAsync(Path.Combine(dir, ".droplet-config.json"));
        var config = JsonSerializer.Deserialize<ConfigOptions>(configJson);

        var projectName = Path.GetFileNameWithoutExtension(path);
        var project = new Project
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

        await _context.AddAsync(project);
        await _context.SaveChangesAsync();
        return project;
    }
}
