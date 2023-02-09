using Command.Share;
using Core.Infrastructure;
using Core.Models;
using Datastore;
using Datastore.Models;

namespace AterStudio.Manager;

public class EntityManager
{
    private readonly DbContext _dbContext;
    public EntityManager(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取实体列表
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public List<EntityFile> GetEntityFiles(Guid id, string? name)
    {
        List<EntityFile> entityFiles = new();
        //var project = await _context.Projects.FindAsync(projectId);
        var project = _dbContext.Projects.FindById(id);
        try
        {
            string entityPath = Path.Combine(project!.EntityPath, "Entities");
            // get files in directory
            List<string> filePaths = Directory.GetFiles(entityPath, "*.cs", SearchOption.AllDirectories).ToList();

            if (filePaths.Any())
            {
                filePaths = filePaths.Where(f => !f.EndsWith(".g.cs")
                    && !f.EndsWith(".AssemblyAttributes.cs")
                    && !f.EndsWith(".AssemblyInfo.cs")
                    && !f.EndsWith("EntityBase.cs")
                    )
                    .ToList();

                foreach (string? path in filePaths)
                {
                    FileInfo file = new(path);
                    EntityFile item = new()
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
        }
        catch (Exception)
        {
            return entityFiles;
        }

        return entityFiles;
    }

    public Project? Find(Guid id)
    {
        return _dbContext.Projects.FindById(id);
    }

    public bool IsExist(Guid id)
    {
        return _dbContext.Projects.FindById(id) != null;
    }

    public async Task GenerateAsync(Project project, GenerateDto dto)
    {
        Const.PROJECT_ID = project.ProjectId;
        CommandRunner.dbContext = _dbContext;
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                await CommandRunner.GenerateDtoAsync(dto.EntityPath, project.SharePath, true);
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
        Const.PROJECT_ID = project.ProjectId;
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateDtoAsync(item, project.SharePath, true);
                }
                break;
            case CommandType.Manager:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateManagerAsync(item, project.SharePath, project.ApplicationPath);
                }

                break;
            case CommandType.API:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateApiAsync(item, project.SharePath, project.ApplicationPath, project.HttpPath, "Controller");
                }
                break;
            case CommandType.Protobuf:
                foreach (string item in dto.EntityPaths)
                {
                    dto.ProjectPath?.ForEach(async p =>
                    {
                        await CommandRunner.GenerateProtobufAsync(item, p);
                    });
                }

                break;
            default:
                break;
        }
    }

    public async Task GenerateRequestAsync(Project project, string webPath, RequestLibType type, string? swaggerPath = null)
    {
        Const.PROJECT_ID = project.ProjectId;
        // 更新选项
        project.WebAppPath = webPath;
        project.SwaggerPath = swaggerPath;
        _dbContext.Projects.Update(project);
        swaggerPath ??= Path.Combine(project.HttpPath, "swagger.json");
        await CommandRunner.GenerateRequestAsync(swaggerPath, webPath, type);
    }

    public async Task GenerateSyncAsync(Project project)
    {
        Const.PROJECT_ID = project.ProjectId;
        string swaggerPath = Path.Combine(project.HttpPath, "swagger.json");
        await CommandRunner.SyncToAngularAsync(swaggerPath, project.EntityPath, project.SharePath, project.HttpPath);
    }
}
