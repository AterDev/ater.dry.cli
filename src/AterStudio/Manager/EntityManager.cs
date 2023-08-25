using System.Text;
using System.Text.RegularExpressions;

using Command.Share;
using Command.Share.Commands;

using Core;
using Core.Infrastructure;
using Core.Infrastructure.Helper;

using Datastore;
using Datastore.Models;
using Microsoft.CodeAnalysis;
using Project = Core.Entities.Project;

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
        try
        {
            Config.EntityPath = Config.EntityPath.Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);
            string entityPath = Path.Combine(_projectContext.SolutionPath!, Config.EntityPath);
            // get files in directory
            List<string> filePaths = Directory.GetFiles(entityPath, "*.cs", SearchOption.AllDirectories).ToList();

            if (filePaths.Any())
            {
                filePaths = filePaths.Where(f => !(f.EndsWith(".g.cs")
                    || f.EndsWith(".AssemblyAttributes.cs")
                    || f.EndsWith(".AssemblyInfo.cs")
                    || f.EndsWith("GlobalUsings.cs")
                    || f.EndsWith("Base.cs"))
                    ).Where(f => Path.GetDirectoryName(f)!.EndsWith("Entities")
                    || Path.GetDirectoryName(f)!.EndsWith("Entity"))
                    .ToList();

                var compilation = new CompilationHelper(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath));
                foreach (string? path in filePaths)
                {
                    FileInfo file = new(path);

                    var content = File.ReadAllText(path);

                    var comment = Regex.Match(content, @"/// <summary>([\s\S]*?)/// <\/summary>")?.Groups[1]?.Value.Trim();
                    comment = comment?.Replace("/", "").Trim();

                    EntityFile item = new()
                    {
                        Name = file.Name,
                        BaseDirPath = entityPath,
                        Path = file.FullName.Replace(entityPath, ""),
                        Content = content,
                        Comment = comment
                    };

                    // 解析特性
                    compilation.AddSyntaxTree(content);
                    var moduleAttribution = compilation.GetClassAttribution("Module");
                    if (moduleAttribution != null && moduleAttribution.Any())
                    {
                        var argument = moduleAttribution.Last().ArgumentList?.Arguments.FirstOrDefault();
                        if (argument != null)
                        {
                            item.Module = compilation.GetArgumentValue(argument);
                        }
                    }

                    // 查询生成的dto\manager\api状态
                    var (hasDto, hasManager, hasAPI) = GetEntityStates(
                        _projectContext.SolutionPath!,
                        Path.GetFileNameWithoutExtension(file.Name),
                        item.Module);

                    item.HasDto = hasDto;
                    item.HasManager = hasManager;
                    item.HasAPI = hasAPI;

                    entityFiles.Add(item);
                }
            }

            // 名称筛选
            if (!string.IsNullOrWhiteSpace(name))
            {
                entityFiles = entityFiles.Where(f => f.Name.ToLower().Contains(name.ToLower()))
                    .ToList();
            }

            // 排序
            entityFiles = entityFiles
                .OrderByDescending(e => e.Module)
                .ThenBy(e => e.Name)
                .ToList();
        }
        catch (Exception)
        {
            return entityFiles;
        }

        return entityFiles;
    }

    /// <summary>
    /// 判断生成状态
    /// </summary>
    /// <param name="path"></param>
    /// <param name="entityName"></param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    private (bool hasDto, bool hasManager, bool hasAPI) GetEntityStates(string path, string entityName, string? moduleName = null)
    {
        bool hasDto = false;
        bool hasManager = false;
        bool hasAPI = false;
        var dtoPath = Path.Combine(path, Config.SharePath, "Models", $"{entityName}Dtos", $"{entityName}AddDto.cs");
        var managerPath = Path.Combine(path, Config.ApplicationPath, "IManager", $"I{entityName}Manager.cs");
        var apiPath = Path.Combine(path, Config.ApiPath, "Controllers", $"{entityName}Controller.cs");

        if (!string.IsNullOrWhiteSpace(moduleName))
        {
            dtoPath = Path.Combine(path, "src", "Modules", moduleName, "Models", $"{entityName}Dtos", $"{entityName}AddDto.cs");
            managerPath = Path.Combine(path, "src", "Modules", moduleName, "IManager", $"I{entityName}Manager.cs");
            apiPath = Path.Combine(path, "src", "Modules", moduleName, "Controllers", $"{entityName}Controller.cs");
        }

        if (File.Exists(dtoPath)) { hasDto = true; }
        if (File.Exists(managerPath)) { hasManager = true; }
        if (File.Exists(apiPath)) { hasAPI = true; }

        return (hasDto, hasManager, hasAPI);
    }

    /// <summary>
    /// 获取实体对应的dto
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <returns></returns>
    public List<EntityFile> GetDtos(string entityFilePath)
    {
        List<EntityFile> dtoFiles = new();
        try
        {
            // 解析特性
            string? moduleName = null;
            var content = File.ReadAllText(entityFilePath);
            var compilation = new CompilationHelper(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath));

            compilation.AddSyntaxTree(content);
            var moduleAttribution = compilation.GetClassAttribution("Module");
            if (moduleAttribution != null && moduleAttribution.Any())
            {
                var argument = moduleAttribution.Last().ArgumentList?.Arguments.FirstOrDefault();
                if (argument != null)
                {
                    moduleName = compilation.GetArgumentValue(argument);
                }
            }

            var entityName = Path.GetFileNameWithoutExtension(entityFilePath);

            string dtoPath = moduleName == null ?
                Path.Combine(_projectContext.SolutionPath!, Config.SharePath, "Models", $"{entityName}Dtos") :
                Path.Combine(_projectContext.SolutionPath!, "src", "Modules", moduleName, "Models", $"{entityName}Dtos");

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

    /// <summary>
    /// 清理解决方案 bin/obj
    /// </summary>
    /// <returns></returns>
    public bool CleanSolution()
    {
        // delete all bin/obj dir  in solution path 
        var dirPaths = new string[] { Config.ApiPath, Config.EntityPath, Config.ApplicationPath, Config.SharePath };
        var dirs = new string[] { };

        foreach (var path in dirPaths)
        {
            var rootPath = Path.Combine(_projectContext.SolutionPath!, path);
            dirs = dirs.Union(Directory.GetDirectories(rootPath, "bin", SearchOption.TopDirectoryOnly))
                .Union(Directory.GetDirectories(rootPath, "obj", SearchOption.TopDirectoryOnly))
                .ToArray();
        }
        try
        {
            foreach (var dir in dirs)
            {
                Directory.Delete(dir, true);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Clean solution occur error:{ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取文件内容
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="isManager"></param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public EntityFile? GetFileContent(string entityName, bool isManager, string? moduleName = null)
    {
        if (entityName.EndsWith(".cs"))
        {
            entityName = entityName.Replace(".cs", "");
        }

        string? filePath;
        if (isManager)
        {
            filePath = Path.Combine(_projectContext.SolutionPath!, Config.ApplicationPath, "Manager", $"{entityName}Manager.cs");

            if (!string.IsNullOrWhiteSpace(moduleName))
            {
                filePath = Path.Combine(_projectContext.SolutionPath!, "src", "Modules", moduleName, "Manager", $"{entityName}Manager.cs");
            }
        }
        else
        {
            var entityDir = Path.Combine(_projectContext.SolutionPath!, Config.EntityPath, "Entities");
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
    /// <param name="fileName"></param>
    /// <param name="Content"></param>
    /// <returns></returns>
    public bool UpdateDtoContent(string fileName, string Content)
    {
        string dtoPath = Path.Combine(_projectContext.SolutionPath!, Config.SharePath, "Models");
        var filePath = Directory.GetFiles(dtoPath, fileName, SearchOption.AllDirectories).FirstOrDefault();
        try
        {
            if (filePath != null)
            {
                File.WriteAllTextAsync(filePath, Content, new UTF8Encoding(false));
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
                    CommandRunner.GenerateApiAsync(item, _projectContext.SharePath!, _projectContext.ApplicationPath!, _projectContext.ApiPath!, "Controller", dto.Force).Wait();
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
        var options = ConfigCommand.ReadConfigFile(_projectContext.SolutionPath!);
        if (options != null)
        {
            options.WebAppPath = webPath;
            options.SwaggerPath = swaggerPath;
            string content = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Path.Combine(_projectContext.SolutionPath!, Config.ConfigFileName), content, new UTF8Encoding(false));
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

    public async Task GenerateNgModuleAsync(string entityName, string rootPath)
    {
        var dtoPath = Path.Combine(_projectContext.SolutionPath!, Config.SharePath);
        var entityDir = Path.Combine(_projectContext.SolutionPath!, Config.EntityPath);
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
