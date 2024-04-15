using System.Diagnostics;
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

public partial class EntityManager(DbContext dbContext, ProjectContext projectContext)
{
    private readonly DbContext _dbContext = dbContext;
    private readonly ProjectContext _projectContext = projectContext;

    /// <summary>
    /// 获取实体列表
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns></returns>
    public List<EntityFile> GetEntityFiles(string? serviceName)
    {
        List<EntityFile> entityFiles = [];
        try
        {
            string entityPath = Config.EntityPath.Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            string servicePath = Path.Combine(_projectContext.SolutionPath!, "src");
            entityPath = Path.Combine(_projectContext.SolutionPath!, entityPath);
            if (!string.IsNullOrWhiteSpace(serviceName))
            {

                servicePath = Path.Combine(_projectContext.SolutionPath!, Config.MicroservicePath, serviceName);
                entityPath = Path.Combine(servicePath, "Definition", "Entity");
            }

            // get files in directory
            List<string> filePaths = [.. Directory.GetFiles(entityPath, "*.cs", SearchOption.AllDirectories)];

            if (filePaths.Count != 0)
            {
                filePaths = filePaths.Where(f => !(f.EndsWith(".g.cs")
                    || f.EndsWith(".AssemblyAttributes.cs")
                    || f.EndsWith(".AssemblyInfo.cs")
                    || f.EndsWith("GlobalUsings.cs")
                    || f.EndsWith("Modules.cs"))
                    ).ToList();

                var compilation = new CompilationHelper(entityPath);
                foreach (string? path in filePaths)
                {
                    FileInfo file = new(path);

                    var content = File.ReadAllText(path);
                    // 备注
                    var comment = SummaryCommentRegex().Match(content)?.Groups[1]?.Value.Trim();
                    comment = comment?.Replace("/", "").Trim();

                    // 解析
                    compilation.AddSyntaxTree(content);
                    // 如果是枚举类，则忽略
                    if (compilation.IsEnum()) continue;

                    EntityFile item = new()
                    {
                        Name = file.Name,
                        BaseDirPath = entityPath,
                        Path = file.FullName.Replace(entityPath, ""),
                        Content = content,
                        Comment = comment
                    };

                    // 解析特性
                    var moduleAttribution = compilation.GetClassAttribution("Module");
                    if (moduleAttribution != null && moduleAttribution.Count != 0)
                    {
                        var argument = moduleAttribution.Last().ArgumentList?.Arguments.FirstOrDefault();
                        if (argument != null)
                        {
                            item.Module = compilation.GetArgumentValue(argument);
                        }
                    }

                    // 查询生成的dto\manager\api状态
                    var (hasDto, hasManager, hasAPI) = GetEntityStates(
                        Path.GetFileNameWithoutExtension(file.Name),
                        serviceName,
                        item.Module);

                    item.HasDto = hasDto;
                    item.HasManager = hasManager;
                    item.HasAPI = hasAPI;
                    entityFiles.Add(item);
                }
            }

            // 排序
            entityFiles = [.. entityFiles.OrderByDescending(e => e.Module).ThenBy(e => e.Name)];
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
    /// <param name="serviceName"></param>
    /// <param name="entityName"></param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    private (bool hasDto, bool hasManager, bool hasAPI) GetEntityStates(string entityName, string? serviceName = null, string? moduleName = null)
    {
        bool hasDto = false;
        bool hasManager = false;
        bool hasAPI = false;

        var dtoPath = Path.Combine(_projectContext.SolutionPath!, Config.SharePath, "Models", $"{entityName}Dtos", $"{entityName}AddDto.cs");
        var managerPath = Path.Combine(_projectContext.SolutionPath!, Config.ApplicationPath, "Manager", $"{entityName}Manager.cs");
        var apiPath = Path.Combine(_projectContext.SolutionPath!, Config.ApiPath, "Controllers", $"{entityName}Controller.cs");
        var servicePath = Path.Combine(_projectContext.SolutionPath!, "src");

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            var serviceOptions = Config.GetServiceConfig(serviceName);
            dtoPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.DtoPath, "Models", $"{entityName}Dtos", $"{entityName}AddDto.cs");
            managerPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApplicationPath, "Manager", $"{entityName}Manager.cs");
            apiPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApiPath, "Controllers", $"{entityName}Controller.cs");
            servicePath = serviceOptions.RootPath;
        }

        if (!string.IsNullOrWhiteSpace(moduleName))
        {
            dtoPath = Path.Combine(servicePath, "Modules", moduleName, "Models", $"{entityName}Dtos", $"{entityName}AddDto.cs");
            managerPath = Path.Combine(servicePath, "Modules", moduleName, "Manager", $"{entityName}Manager.cs");
            apiPath = Path.Combine(servicePath, "Modules", moduleName, "Controllers", $"{entityName}Controller.cs");
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
        List<EntityFile> dtoFiles = [];
        try
        {
            string dtoPath = GetDtoPath(entityFilePath);

            // get files in directory
            List<string> filePaths = [.. Directory.GetFiles(dtoPath, "*.cs", SearchOption.AllDirectories)];

            if (filePaths.Count != 0)
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

    private string GetDtoPath(string entityFilePath)
    {
        // 解析特性
        string? moduleName = null;
        var content = File.ReadAllText(entityFilePath);
        var compilation = new CompilationHelper(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath));

        compilation.AddSyntaxTree(content);
        var moduleAttribution = compilation.GetClassAttribution("Module");
        if (moduleAttribution != null && moduleAttribution.Count != 0)
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
        return dtoPath;
    }

    /// <summary>
    /// 清理解决方案 bin/obj
    /// </summary>
    /// <returns></returns>
    public bool CleanSolution(out string errorMsg)
    {
        errorMsg = string.Empty;
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
            var apiFiePath = Directory.GetFiles(_projectContext.ApiPath!, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (apiFiePath != null)
            {
                Console.WriteLine($"⛏️ build project:{apiFiePath}");
                var process = Process.Start("dotnet", $"build {apiFiePath}");
                process.WaitForExit();
                // if process has error message 
                if (process.ExitCode != 0)
                {
                    errorMsg = "项目构建失败，请检查项目！";
                    return false;
                }
                return true;
            }
            errorMsg = "未找到API项目，清理后请手动重新构建项目!";
            return false;
        }
        catch (Exception ex)
        {
            errorMsg = "项目清理失败，请尝试关闭占用程序后重试.";
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
    /// <param name="filePath"></param>
    /// <param name="Content"></param>
    /// <returns></returns>
    public bool UpdateDtoContent(string filePath, string Content)
    {
        try
        {
            if (filePath != null)
            {
                File.WriteAllTextAsync(filePath, Content, new UTF8Encoding(false));
                return true;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
        var dtoPath = _projectContext.SharePath!;
        var applicationPath = _projectContext.ApplicationPath!;
        var apiPath = _projectContext.ApiPath!;

        if (!string.IsNullOrWhiteSpace(dto.ServiceName))
        {
            Config.IsSplitController = false;
            Config.IsMicroservice = true;
            Config.ServiceName = dto.ServiceName;
            var serviceOptions = Config.GetServiceConfig(dto.ServiceName);
            dtoPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.DtoPath);
            applicationPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApplicationPath);
            apiPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApiPath);
        }
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                await CommandRunner.GenerateDtoAsync(dto.EntityPath, dtoPath, dto.Force);
                break;
            case CommandType.Manager:
                await CommandRunner.GenerateManagerAsync(dto.EntityPath, dtoPath, applicationPath, dto.Force);
                break;
            case CommandType.API:
                await CommandRunner.GenerateApiAsync(dto.EntityPath, dtoPath, applicationPath, apiPath, "Controller", dto.Force);
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
        var dtoPath = _projectContext.SharePath!;
        var applicationPath = _projectContext.ApplicationPath!;
        var apiPath = _projectContext.ApiPath!;
        var entityPath = _projectContext.EntityPath!;

        if (!string.IsNullOrWhiteSpace(dto.ServiceName))
        {
            Config.IsSplitController = false;
            Config.IsMicroservice = true;
            Config.ServiceName = dto.ServiceName;
            var serviceOptions = Config.GetServiceConfig(dto.ServiceName);
            dtoPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.DtoPath);
            applicationPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApplicationPath);
            apiPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApiPath);
            entityPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.EntityPath);
        }
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateDtoAsync(item, dtoPath, dto.Force);
                }
                break;
            case CommandType.Manager:
                foreach (string item in dto.EntityPaths)
                {
                    await CommandRunner.GenerateManagerAsync(item, dtoPath, applicationPath, dto.Force);
                }

                break;
            case CommandType.API:
                foreach (string item in dto.EntityPaths)
                {
                    CommandRunner.GenerateApiAsync(item, dtoPath, applicationPath, apiPath, "Controller", dto.Force).Wait();
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
                    await CommandRunner.ClearCodesAsync(entityPath, dtoPath, applicationPath, apiPath, entityName);
                }
                break;
            default:
                break;
        }
    }

    public async Task GenerateRequestAsync(string webPath, RequestLibType type, string? swaggerPath = null)
    {
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

    /// <summary>
    /// 创建dto
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <param name="name"></param>
    /// <param name="summary"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string?> CreateDtoAsync(string entityFilePath, string name, string summary = "")
    {
        // 解析特性
        string? moduleName = null;
        var content = File.ReadAllText(entityFilePath);
        var compilation = new CompilationHelper(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath));

        compilation.AddSyntaxTree(content);
        var moduleAttribution = compilation.GetClassAttribution("Module");
        if (moduleAttribution != null && moduleAttribution.Count != 0)
        {
            var argument = moduleAttribution.Last().ArgumentList?.Arguments.FirstOrDefault();
            if (argument != null)
            {
                moduleName = compilation.GetArgumentValue(argument);
            }
        }
        var entityHelper = new EntityParseHelper(entityFilePath);
        var entityInfo = entityHelper.GetEntity();

        var entityName = Path.GetFileNameWithoutExtension(entityFilePath);

        string dtoPath = moduleName == null ?
            Path.Combine(_projectContext.SolutionPath!, Config.SharePath, "Models", $"{entityName}Dtos") :
            Path.Combine(_projectContext.SolutionPath!, "src", "Modules", moduleName, "Models", $"{entityName}Dtos");

        if (Directory.Exists(dtoPath))
        {
            var fileName = name;
            if (!fileName.EndsWith(".cs"))
            {
                fileName += ".cs";
            }
            dtoPath = Path.Combine(dtoPath, fileName);

            if (File.Exists(dtoPath))
            {
                return dtoPath;
            }

            Console.WriteLine($"🆕 Create new Dto:{dtoPath}");

            string nspName = moduleName == null ?
                $"namespace Share.Models.{entityName}Dtos;" :
                $"namespace {moduleName}.Models.{entityName}Dtos;";
            content = $$"""
                {{nspName}}
                /// <summary>
                /// {{summary}}
                /// </summary>
                /// <see cref="{{entityInfo.NamespaceName}}.{{entityName}}"/>
                public class {{name}}
                {
                    

                }
                
                """;
            await File.WriteAllTextAsync(dtoPath, content, new UTF8Encoding(false));
            return dtoPath;
        }
        return null;
    }

    [GeneratedRegex(@"/// <summary>([\s\S]*?)/// <\/summary>")]
    private static partial Regex SummaryCommentRegex();
}
