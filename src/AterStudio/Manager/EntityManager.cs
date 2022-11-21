using AterStudio.Entity;
using AterStudio.Models;
using Droplet.CommandLine;

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
                await CommandRunner.GenerateApiAsync(dto.EntityPath, project.SharePath, project.ApplicationPath);
                break;
            default:
                break;
        }

    }
}
