using System.Text;

using Command.Share;
using Command.Share.Commands;

using Core;
using Core.Entities;
using Core.Infrastructure;

using Datastore;
using Datastore.Models;

namespace AterStudio.Manager;

public class EntityManager
{
    private readonly DbContext _dbContext;
    private readonly ProjectContext _projectContext;
    public EntityManager(DbContext dbContext, ProjectContext projectContext)
    {
        _dbContext = dbContext;
        _projectContext = projectContext;
    }

    /// <summary>
    /// 获取实体列表
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public List<EntityFile> GetEntityFiles(string? name)
    {
        List<EntityFile> entityFiles = new();
        //var project = await _context.Projects.FindAsync(projectId);
        try
        {

            Config.EntityPath = Config.EntityPath.Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);
            string entityPath = Path.Combine(_projectContext.ProjectPath!, Config.EntityPath, "Entities");
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
                    var states = GetEntityStates(_projectContext.ProjectPath!, Path.GetFileNameWithoutExtension(file.Name));
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
        bool hasDto = false; bool hasManager = false; bool hasAPI = false;
        var dtoPath = Path.Combine(path, Config.DtoPath, "Models", $"{entityName}Dtos", $"{entityName}AddDto.cs");
        var managerPath = Path.Combine(path, Config.StorePath, "IManager", $"I{entityName}Manager.cs");
        var apiPath = Path.Combine(path, Config.ApiPath, "Controllers", $"{entityName}Controller.cs");

        if (File.Exists(dtoPath)) { hasDto = true; }
        if (File.Exists(managerPath)) { hasManager = true; }
        if (File.Exists(apiPath)) { hasAPI = true; }

        return (hasDto, hasManager, hasAPI);
    }

    /// <summary>
    /// 获取实体对应的dto
    /// </summary>
    /// <param name="entityName"></param>
    /// <returns></returns>
    public List<EntityFile> GetDtos(string entityName)
    {
        List<EntityFile> dtoFiles = new();
        try
        {
            // dto目录
            if (entityName.EndsWith(".cs"))
            {
                entityName = entityName.Replace(".cs", "");
            }
            string dtoPath = Path.Combine(_projectContext.ProjectPath!, Config.DtoPath, "Models", $"{entityName}Dtos");
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


    public EntityFile? GetFileContent(string entityName, bool isManager)
    {
        if (entityName.EndsWith(".cs"))
        {
            entityName = entityName.Replace(".cs", "");
        }

        string? filePath = null;
        if (isManager)
        {
            filePath = Path.Combine(_projectContext.ProjectPath!, Config.StorePath, "Manager", $"{entityName}Manager.cs");
        }
        else
        {
            var entityDir = Path.Combine(_projectContext.ProjectPath!, Config.EntityPath, "Entities");
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
        string dtoPath = Path.Combine(_projectContext.ProjectPath!, Config.DtoPath, "Models");
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
        CommandRunner.dbContext = _dbContext;
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                await CommandRunner.GenerateDtoAsync(dto.EntityPath, _projectContext.SharePath!, dto.Force);
                break;
            case CommandType.Manager:
                await CommandRunner.GenerateManagerAsync(dto.EntityPath, _projectContext.SharePath!, _projectContext.ApplicationPath!, dto.Force);
                break;
            case CommandType.API:
                await CommandRunner.GenerateApiAsync(dto.EntityPath, _projectContext.SharePath!, _projectContext.ApplicationPath!, _projectContext.ApiPath!, "Controller", dto.Force);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 批量生成
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task BatchGenerateAsync(BatchGenerateDto dto)
    {
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateDtoAsync(item, _projectContext.SharePath!, dto.Force);
                }
                break;
            case CommandType.Manager:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateManagerAsync(item, _projectContext.SharePath!, _projectContext.ApplicationPath!, dto.Force);
                }

                break;
            case CommandType.API:
                foreach (string item in dto.EntityPaths)
                {
                    dto.ProjectPath?.ForEach(p =>
                    {
                        CommandRunner.GenerateApiAsync(item, _projectContext.SharePath!, _projectContext.ApplicationPath!, _projectContext.ApiPath!, "Controller", dto.Force).Wait();
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
            case CommandType.Clear:
                foreach (string item in dto.EntityPaths)
                {
                    var entityName = Path.GetFileNameWithoutExtension(item);
                    await CommandRunner.ClearCodesAsync(_projectContext.EntityPath!, _projectContext.SharePath!, _projectContext.ApplicationPath!, _projectContext.ApiPath!, entityName);
                }
                break;
            default:
                break;
        }
    }

    public async Task GenerateRequestAsync(string webPath, RequestLibType type, string? swaggerPath = null)
    {
        // 更新选项
        var options = ConfigCommand.ReadConfigFile(_projectContext.ProjectPath!);
        if (options != null)
        {
            options.WebAppPath = webPath;
            options.SwaggerPath = swaggerPath;
            string content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Path.Combine(_projectContext.ProjectPath!, Config.ConfigFileName), content, Encoding.UTF8);
        }

        swaggerPath ??= Path.Combine(_projectContext.ApiPath!, "swagger.json");
        await CommandRunner.GenerateRequestAsync(swaggerPath, webPath, type);
    }


    public async Task GenerateClientRequestAsync(Project project, string webPath, LanguageType type, string? swaggerPath = null)
    {
        swaggerPath ??= Path.Combine(_projectContext.ApiPath!, "swagger.json");
        await CommandRunner.GenerateCSharpApiClientAsync(swaggerPath, webPath, type);
    }

    public async Task GenerateSyncAsync(Project project)
    {
        Const.PROJECT_ID = project.ProjectId;
        string swaggerPath = Path.Combine(_projectContext.ApiPath!, "swagger.json");
        await CommandRunner.SyncToAngularAsync(swaggerPath, _projectContext.EntityPath!, _projectContext.SharePath!, _projectContext.ApiPath!);
    }

    public async Task GenerateNgModuleAsync(Project project, string entityName, string rootPath)
    {
        var dtoPath = Path.Combine(project.Path, "..", "src", Config.DtoPath);
        var entityDir = Path.Combine(project.Path, "..", "src", Config.EntityPath, "Entities");
        var entityPath = Directory.GetFiles(entityDir, entityName, SearchOption.AllDirectories)
            .FirstOrDefault();

        if (entityPath != null)
        {
            await CommandRunner.GenerateNgPagesAsync(entityPath, dtoPath, rootPath);
        }
        else
        {
            throw new FileNotFoundException($"未找到实体文件:{entityPath}");
        }
    }


}
