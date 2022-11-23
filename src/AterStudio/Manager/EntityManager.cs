using AterStudio.Entity;
using AterStudio.Models;
using Command.Share;

namespace AterStudio.Manager;

public class EntityManager
{
    private readonly ContextBase _context;
    public EntityManager(ContextBase context)
    {
        _context = context;
    }


    public async Task<List<EntityFile>> GetEntityFilesAsync(int projectId)
    {
        var entityFiles = new List<EntityFile>();
        var project = await _context.Projects.FindAsync(projectId);
        var entityPath = Path.Combine(project.EntityPath, "Entities");
        // get files in directory
        var filePaths = Directory.GetFiles(entityPath, "*.cs", SearchOption.AllDirectories).ToList();

        if (filePaths.Any())
        {
            filePaths = filePaths.Where(f => !f.EndsWith(".g.cs")
                && !f.EndsWith(".AssemblyAttributes.cs")
                && !f.EndsWith(".AssemblyInfo.cs")
                && !f.EndsWith("EntityBase.cs")
                )
                .ToList();

            foreach (var path in filePaths)
            {
                var file = new FileInfo(path);
                var item = new EntityFile
                {
                    Name = file.Name,
                    Path = file.FullName,
                    Content = File.ReadAllText(path)
                };

                entityFiles.Add(item);
            }
        }
        return entityFiles;
    }

    public async Task GenerateAsync(Project project, GenerateDto dto)
    {
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                await CommandRunner.GenerateDtoAsync(dto.EntityPath, project.SharePath, false);
                break;
            case CommandType.Manager:
                await CommandRunner.GenerateManagerAsync(dto.EntityPath, project.SharePath, project.ApplicationPath);
                break;
            case CommandType.API:
                await CommandRunner.GenerateApiAsync(dto.EntityPath, project.SharePath, project.ApplicationPath, project.HttpPath, "Controller");
                break;
            default:
                break;
        }
    }

    public async Task GenerateRequestAsync(Project project, string webPath, RequestLibType type)
    {
        var swaggerPath = Path.Combine(project.HttpPath, "swagger.json");
        await CommandRunner.GenerateRequestAsync(swaggerPath, webPath, type);
    }

    public async Task GenerateSyncAsync(Project project)
    {
        var swaggerPath = Path.Combine(project.HttpPath, "swagger.json");
        await CommandRunner.SyncToAngularAsync(swaggerPath, project.EntityPath, project.SharePath, project.HttpPath);
    }
}
