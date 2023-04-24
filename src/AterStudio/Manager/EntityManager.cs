using System.Text;
using Command.Share;
using Command.Share.Commands;
using Core;
using Core.Infrastructure;
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
                    // 查询生成的dto\manager\api状态
                    var states = GetEntityStates(project.Path, Path.GetFileNameWithoutExtension(file.Name));
                    item.HasDto = states.hasDto;
                    item.HasManager = states.hasManager;
                    item.HasAPI = states.hasAPI;

                    entityFiles.Add(item);
                }
            }

            // 名称筛选
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

    private (bool hasDto, bool hasManager, bool hasAPI) GetEntityStates(string path, string entityName)
    {
        var basePath = path;
        if (File.Exists(path))
        {
            basePath = Path.Combine(path, "..");
        }

        bool hasDto = false; bool hasManager = false; bool hasAPI = false;
        var dtoPath = Path.Combine(basePath, "src", Config.DtoPath, "Models", $"{entityName}Dtos", $"{entityName}AddDto.cs");
        var managerPath = Path.Combine(basePath, "src", Config.StorePath, "IManager", $"I{entityName}Manager.cs");
        var apiPath = Path.Combine(basePath, "src", Config.ApiPath, "Controllers", $"{entityName}Controller.cs");

        if (File.Exists(dtoPath)) { hasDto = true; }
        if (File.Exists(managerPath)) { hasManager = true; }
        if (File.Exists(apiPath)) { hasAPI = true; }

        return (hasDto, hasManager, hasAPI);
    }

    /// <summary>
    /// 获取实体对应的dto
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="entityName"></param>
    /// <returns></returns>
    public List<EntityFile> GetDtos(Guid projectId, string entityName)
    {
        List<EntityFile> dtoFiles = new();
        //var project = await _context.Projects.FindAsync(projectId);
        var project = _dbContext.Projects.FindById(projectId);
        try
        {
            // dto目录
            if (entityName.EndsWith(".cs"))
            {
                entityName = entityName.Replace(".cs", "");
            }
            string dtoPath = Path.Combine(project!.SharePath, "Models", $"{entityName}Dtos");
            // get files in directory
            List<string> filePaths = Directory.GetFiles(dtoPath, "*.cs", SearchOption.AllDirectories).ToList();

            if (filePaths.Any())
            {
                filePaths = filePaths.Where(f => !f.EndsWith(".g.cs"))
                    .ToList();

                foreach (string? path in filePaths)
                {
                    FileInfo file = new(path);
                    EntityFile item = new()
                    {
                        Name = file.Name,
                        BaseDirPath = dtoPath,
                        Path = file.FullName.Replace(dtoPath, ""),
                        Content = File.ReadAllText(path)
                    };

                    dtoFiles.Add(item);
                }
            }
        }
        catch (Exception)
        {
            return dtoFiles;
        }
        return dtoFiles;
    }


    public EntityFile? GetFileContent(Guid projectId, string entityName, bool isManager)
    {
        var project = _dbContext.Projects.FindById(projectId);
        if (entityName.EndsWith(".cs"))
        {
            entityName = entityName.Replace(".cs", "");
        }

        string? filePath = null;
        if (isManager)
        {
            filePath = Path.Combine(project!.ApplicationPath, "Manager", $"{entityName}Manager.cs");
        }
        else
        {
            var entityDir = Path.Combine(project!.EntityPath, "Entities");
            filePath = Directory.GetFiles(entityDir, $"{entityName}.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
        }
        if (filePath != null)
        {
            var file = new FileInfo(filePath);

            return new EntityFile()
            {
                Name = file.Name,
                BaseDirPath = file.DirectoryName ?? "",
                Path = file.FullName,
                Content = File.ReadAllText(filePath)
            };

        }

        return default;
    }

    /// <summary>
    /// 保存Dto内容
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="fileName"></param>
    /// <param name="Content"></param>
    /// <returns></returns>
    public bool UpdateDtoContent(Guid projectId, string fileName, string Content)
    {
        var project = _dbContext.Projects.FindById(projectId);
        string dtoPath = Path.Combine(project!.SharePath, "Models");
        var filePath = Directory.GetFiles(dtoPath, fileName, SearchOption.AllDirectories).FirstOrDefault();
        try
        {
            if (filePath != null)
            {
                File.WriteAllTextAsync(filePath, Content, Encoding.UTF8);
                return true;
            }

        }
        catch (Exception)
        {
            return false;
        }
        return false;
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
        await InitConfigAsync(project);
        Const.PROJECT_ID = project.ProjectId;
        CommandRunner.dbContext = _dbContext;
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                await CommandRunner.GenerateDtoAsync(dto.EntityPath, project.SharePath, dto.Force);
                break;
            case CommandType.Manager:
                await CommandRunner.GenerateManagerAsync(dto.EntityPath, project.SharePath, project.ApplicationPath, dto.Force);
                break;
            case CommandType.API:
                await CommandRunner.GenerateApiAsync(dto.EntityPath, project.SharePath, project.ApplicationPath, project.HttpPath, "Controller", dto.Force);
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
        await InitConfigAsync(project);
        Const.PROJECT_ID = project.ProjectId;
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateDtoAsync(item, project.SharePath, dto.Force);
                }
                break;
            case CommandType.Manager:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateManagerAsync(item, project.SharePath, project.ApplicationPath, dto.Force);
                }

                break;
            case CommandType.API:
                foreach (string item in dto.EntityPaths)
                {
                    dto.ProjectPath?.ForEach(p =>
                    {
                        var apiPath = Path.Combine(p, "..");
                        CommandRunner.GenerateApiAsync(item, project.SharePath, project.ApplicationPath, apiPath, "Controller", dto.Force).Wait();
                    });

                }
                break;
            case CommandType.Protobuf:
                foreach (string item in dto.EntityPaths)
                {
                    dto.ProjectPath?.ForEach(p =>
                    {
                        _ = CommandRunner.GenerateProtobufAsync(item, p).Result;
                    });
                }
                break;
            default:
                break;
        }
    }

    public async Task GenerateRequestAsync(Project project, string webPath, RequestLibType type, string? swaggerPath = null)
    {
        await InitConfigAsync(project);
        Const.PROJECT_ID = project.ProjectId;
        // 更新选项
        project.WebAppPath = webPath;
        project.SwaggerPath = swaggerPath;
        _dbContext.Projects.Update(project);

        swaggerPath ??= Path.Combine(project.HttpPath, "swagger.json");
        await CommandRunner.GenerateRequestAsync(swaggerPath, webPath, type);
    }


    public async Task GenerateClientRequestAsync(Project project, string webPath, LanguageType type, string? swaggerPath = null)
    {
        await InitConfigAsync(project);
        Const.PROJECT_ID = project.ProjectId;
        swaggerPath ??= Path.Combine(project.HttpPath, "swagger.json");
        await CommandRunner.GenerateCSharpApiClientAsync(swaggerPath, webPath, type);
    }

    public async Task GenerateSyncAsync(Project project)
    {
        await InitConfigAsync(project);
        Const.PROJECT_ID = project.ProjectId;
        string swaggerPath = Path.Combine(project.HttpPath, "swagger.json");
        await CommandRunner.SyncToAngularAsync(swaggerPath, project.EntityPath, project.SharePath, project.HttpPath);
    }

    private async Task InitConfigAsync(Project project)
    {
        var slnFile = new FileInfo(project.Path);
        // 如果是目录
        string? configFile = (slnFile.Attributes & FileAttributes.Directory) != 0
            ? Path.Combine(project.Path)
            : Path.Combine(slnFile.DirectoryName!);

        await ConfigCommand.InitConfigFileAsync(configFile);
        var options = ConfigCommand.ReadConfigFile(configFile);

        if (options != null)
        {
            Config.SetConfig(options);
        }
    }
}
