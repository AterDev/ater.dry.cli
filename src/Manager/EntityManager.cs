using CodeGenerator.Generate;
using Command.Share;
using Manager.Entity;
using Manager.Models;

namespace Manager;

public class EntityManager
{
    private readonly ContextBase _context;
    public EntityManager(ContextBase context)
    {
        _context = context;
    }


    /// <summary>
    /// 获取实体列表
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<List<EntityFile>> GetEntityFilesAsync(int projectId, string? name)
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
                    BaseDirPath = entityPath,
                    Path = file.FullName.Replace(entityPath, ""),
                    Content = File.ReadAllText(path)
                };

                entityFiles.Add(item);
            }
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            entityFiles = entityFiles.Where(f => f.Name.ToLower().Contains(name.ToLower())).ToList();
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

    /// <summary>
    /// 批量生成
    /// </summary>
    /// <param name="project"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task BatchGenerateAsync(Project project, BatchGenerateDto dto)
    {
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                foreach (var item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateDtoAsync(item, project.SharePath, false);
                }
                break;
            case CommandType.Manager:
                foreach (var item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateManagerAsync(item, project.SharePath, project.ApplicationPath);
                }

                break;
            case CommandType.API:
                foreach (var item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateApiAsync(item, project.SharePath, project.ApplicationPath, project.HttpPath, "Controller");
                }
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
