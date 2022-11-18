using AterStudio.Models;

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
        var entityPath = project.EntityPath;
        // get files in directory
        var filePaths = Directory.GetFiles(entityPath, "*.cs", SearchOption.AllDirectories).ToList();

        if (filePaths.Any())
        {

            filePaths = filePaths.Where(f => !f.EndsWith(".g.cs") 
                && !f.EndsWith(".AssemblyAttributes.cs")
                && !f.EndsWith(".AssemblyInfo.cs")
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
}
