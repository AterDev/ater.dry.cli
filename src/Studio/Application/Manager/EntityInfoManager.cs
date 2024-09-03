using System.Diagnostics;
using System.Text;
using CodeGenerator.Helper;
using Microsoft.CodeAnalysis;
using Share;
using Share.Models;
using Share.Services;

using Project = Entity.Project;

namespace Application.Manager;

public partial class EntityInfoManager(
    DataAccessContext<EntityInfo> dataContext,
    ILogger<EntityInfoManager> logger,
    CodeAnalysisService codeAnalysis,
    IProjectContext projectContext)
    : ManagerBase<EntityInfo>(dataContext, logger)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CodeAnalysisService _codeAnalysis = codeAnalysis;

    /// <summary>
    /// 获取实体列表
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns></returns>
    public List<EntityFile> GetEntityFiles(string entityPath)
    {
        List<EntityFile> entityFiles = [];
        try
        {
            var filePaths = CodeAnalysisService.GetEntityFilePaths(entityPath);

            if (filePaths.Count != 0)
            {
                entityFiles = CodeAnalysisService.GetEntityFiles(filePaths);
                foreach (var item in entityFiles)
                {
                    // 查询生成的dto\manager\api状态
                    (bool hasDto, bool hasManager, bool hasAPI) = GetEntityStates(item);
                    item.HasDto = hasDto;
                    item.HasManager = hasManager;
                    item.HasAPI = hasAPI;
                    entityFiles.Add(item);
                }
                // 排序
                entityFiles = [.. entityFiles.OrderByDescending(e => e.ModuleName).ThenBy(e => e.Name)];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
    private (bool hasDto, bool hasManager, bool hasAPI) GetEntityStates(EntityFile entity)
    {
        bool hasDto = false;
        bool hasManager = false;
        bool hasAPI = false;

        string dtoPath = Path.Combine(entity.GetDtoPath(_projectContext.SolutionPath!), $"{entity.Name}AddDto.cs");
        string managerPath = Path.Combine(entity.GetManagerPath(_projectContext.SolutionPath!), $"{entity.Name}Manager.cs");
        string apiPath = Path.Combine(entity.GetControllerPath(_projectContext.SolutionPath!), "Controllers");

        string servicePath = Path.Combine(_projectContext.SolutionPath!, "src");

        if (Directory.Exists(apiPath))
        {
            string[] apiFiles = Directory.GetFiles(apiPath, $"{entity.Name}Controller.cs", SearchOption.AllDirectories);
            if (apiFiles.Count() > 0) { hasAPI = true; }
        }

        if (File.Exists(dtoPath)) { hasDto = true; }
        if (File.Exists(managerPath)) { hasManager = true; }
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
                        FullName = file.FullName.Replace(dtoPath, ""),
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

    private string? GetDtoPath(string entityFilePath)
    {
        var entityFile = CodeAnalysisService.GetEntityFile(entityFilePath);
        return entityFile?.GetDtoPath(_projectContext.SolutionPath!);
    }

    /// <summary>
    /// 清理解决方案 bin/obj
    /// </summary>
    /// <returns></returns>
    public bool CleanSolution(out string errorMsg)
    {
        errorMsg = string.Empty;
        // delete all bin/obj dir  in solution path 
        string?[] dirPaths = [
            _projectContext.ApiPath,
            _projectContext.EntityPath,
            _projectContext.EntityFrameworkPath,
            _projectContext.ApplicationPath,
            _projectContext.SharePath,
            _projectContext.ModulesPath
            ];

        string[] dirs = [];

        foreach (string path in dirPaths.Where(p => p.NotEmpty()))
        {
            string rootPath = Path.Combine(_projectContext.SolutionPath!, path);
            dirs = dirs.Union(Directory.GetDirectories(rootPath, "bin", SearchOption.TopDirectoryOnly))
                .Union(Directory.GetDirectories(rootPath, "obj", SearchOption.TopDirectoryOnly))
                .ToArray();
        }
        try
        {
            foreach (string dir in dirs)
            {
                Directory.Delete(dir, true);
            }
            string? apiFiePath = Directory.GetFiles(_projectContext.ApiPath!, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (apiFiePath != null)
            {
                Console.WriteLine($"⛏️ build project:{apiFiePath}");
                Process process = Process.Start("dotnet", $"build {apiFiePath}");
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
            string entityDir = Path.Combine(_projectContext.SolutionPath!, Config.EntityPath, "Entities");
            filePath = Directory.GetFiles(entityDir, $"{entityName}.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
        }
        if (filePath != null)
        {
            FileInfo file = new(filePath);

            return new EntityFile()
            {
                Name = file.Name,
                BaseDirPath = file.DirectoryName ?? "",
                FullName = file.FullName,
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


    public async Task GenerateAsync(Project project, GenerateDto dto)
    {
        string dtoPath = _projectContext.SharePath!;
        string applicationPath = _projectContext.ApplicationPath!;
        string apiPath = _projectContext.ApiPath!;

        if (!string.IsNullOrWhiteSpace(dto.ServiceName))
        {
            Config.IsMicroservice = true;
            Config.ServiceName = dto.ServiceName;
            var serviceOptions = Config.GetServiceConfig(dto.ServiceName);
            dtoPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.DtoPath);
            applicationPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApplicationPath);
            apiPath = Path.Combine(_projectContext.SolutionPath!, serviceOptions.ApiPath);
            Config.IsMicroservice = false;

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
        string dtoPath = _projectContext.SharePath!;
        string applicationPath = _projectContext.ApplicationPath!;
        string apiPath = _projectContext.ApiPath!;
        string entityPath = _projectContext.EntityPath!;

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
                    string entityName = Path.GetFileNameWithoutExtension(item);
                    await CommandRunner.ClearCodesAsync(entityPath, dtoPath, applicationPath, apiPath, entityName);
                }
                break;
            default:
                break;
        }
    }

    public async Task GenerateSyncAsync(Project project)
    {
        string swaggerPath = Path.Combine(_projectContext.ApiPath!, "swagger.json");
        await CommandRunner.SyncToAngularAsync(swaggerPath, _projectContext.EntityPath!, _projectContext.SharePath!, _projectContext.ApiPath!);
    }

    /// <summary>
    /// 生成NG组件页面
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="rootPath"></param>
    /// <param name="isMobile"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task GenerateNgModuleAsync(string entityName, string rootPath, bool isMobile = false)
    {
        string dtoPath = Path.Combine(_projectContext.SolutionPath!, Config.SharePath);
        string entityDir = Path.Combine(_projectContext.SolutionPath!, Config.EntityPath);
        string? entityPath = Directory.GetFiles(entityDir, entityName, SearchOption.AllDirectories)
            .FirstOrDefault();

        Entity.Project? project = CommandContext.Projects.Find(_projectContext.ProjectId);
        project!.FrontPath = rootPath;
        await CommandContext.SaveChangesAsync();

        if (entityPath != null)
        {
            await CommandRunner.GenerateNgPagesAsync(entityPath, dtoPath, rootPath, isMobile);
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
        string content = File.ReadAllText(entityFilePath);
        var compilation = new CompilationHelper(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath));

        compilation.LoadContent(content);
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

        string entityName = Path.GetFileNameWithoutExtension(entityFilePath);

        string dtoPath = moduleName == null ?
            Path.Combine(_projectContext.SolutionPath!, Config.SharePath, "Models", $"{entityName}Dtos") :
            Path.Combine(_projectContext.SolutionPath!, "src", "Modules", moduleName, "Models", $"{entityName}Dtos");

        if (Directory.Exists(dtoPath))
        {
            string fileName = name;
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
}
