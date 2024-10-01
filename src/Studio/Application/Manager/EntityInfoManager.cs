using System.Diagnostics;
using CodeGenerator.Models;
using Microsoft.CodeAnalysis;

namespace Application.Manager;

public partial class EntityInfoManager(
    DataAccessContext<EntityInfo> dataContext,
    ILogger<EntityInfoManager> logger,
    CodeAnalysisService codeAnalysis,
    CodeGenService codeGenService,
    IProjectContext projectContext)
    : ManagerBase<EntityInfo>(dataContext, logger)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CodeAnalysisService _codeAnalysis = codeAnalysis;
    private readonly CodeGenService _codeGenService = codeGenService;

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
                entityFiles = _codeAnalysis.GetEntityFiles(filePaths);
                foreach (var item in entityFiles)
                {
                    // 查询生成的dto\manager\api状态
                    (bool hasDto, bool hasManager, bool hasAPI) = GetEntityStates(item);
                    item.HasDto = hasDto;
                    item.HasManager = hasManager;
                    item.HasAPI = hasAPI;
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
        var entityName = Path.GetFileNameWithoutExtension(entity.Name);

        string dtoPath = Path.Combine(entity.GetDtoPath(_projectContext), $"{entityName}AddDto.cs");
        string managerPath = Path.Combine(entity.GetManagerPath(_projectContext), $"{entityName}Manager.cs");
        string apiPath = Path.Combine(entity.GetControllerPath(_projectContext));

        string servicePath = Path.Combine(_projectContext.SolutionPath!, "src");

        if (Directory.Exists(apiPath))
        {
            if (File.Exists(Path.Combine(apiPath, $"{entityName}Controller.cs")) ||
                File.Exists(Path.Combine(apiPath, "AdminControllers", $"{entityName}Controller.cs")))
            {
                hasAPI = true;
            }
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
        var entityFile = _codeAnalysis.GetEntityFile(entityFilePath);
        return entityFile?.GetDtoPath(_projectContext);
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
        var entityFile = new EntityFile
        {
            Name = entityName,
            FullName = entityName,
            ModuleName = moduleName
        };

        string? filePath;
        if (isManager)
        {

            filePath = entityFile.GetManagerPath(_projectContext);
            filePath = Path.Combine(filePath, $"{entityName}Manager.cs");
        }
        else
        {
            string entityDir = Path.Combine(_projectContext.EntityPath!);
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
    public async Task<bool> UpdateDtoContentAsync(string filePath, string Content)
    {
        try
        {
            if (filePath != null)
            {
                await File.WriteAllTextAsync(filePath, Content, Encoding.UTF8);
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

    /// <summary>
    /// 生成服务
    /// </summary>
    /// <param name="project"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task GenerateAsync(GenerateDto dto)
    {
        string dtoPath = _projectContext.SharePath!;
        string applicationPath = _projectContext.ApplicationPath!;
        string apiPath = _projectContext.ApiPath!;

        var helper = new EntityParseHelper(dto.EntityPath);
        var entityInfo = await helper.ParseEntityAsync();

        if (entityInfo == null)
        {
            throw new Exception("实体解析失败，请检查实体文件是否正确！");
        }

        var files = new List<GenFileInfo>();
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                files = _codeGenService.GenerateDto(entityInfo, dtoPath, dto.Force);
                break;
            case CommandType.Manager:
                files = _codeGenService.GenerateManager(entityInfo, applicationPath, dtoPath, dto.Force);
                break;
            case CommandType.API:
                files = _codeGenService.GenerateController(entityInfo, apiPath, dtoPath, dto.Force);
                break;
            default:
                break;
        }
        _codeGenService.GenerateFiles(files);
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

        var files = new List<GenFileInfo>();
        var entityInfos = CodeAnalysisService.GetEntityInfos(dto.EntityPaths) ?? [];
        switch (dto.CommandType)
        {
            case CommandType.Dto:
                foreach (var item in entityInfos)
                {
                    files.AddRange(_codeGenService.GenerateDto(item, dtoPath, dto.Force));
                }
                break;
            case CommandType.Manager:
                foreach (var item in entityInfos)
                {

                    files.AddRange(_codeGenService.GenerateManager(item, applicationPath, "", dto.Force));
                }

                break;
            case CommandType.API:
                foreach (var item in entityInfos)
                {

                    files.AddRange(_codeGenService.GenerateController(item, apiPath, "", dto.Force));
                }
                break;
            case CommandType.Protobuf:
                foreach (string item in dto.EntityPaths)
                {
                    dto.ProjectPath?.ForEach(p =>
                    {
                    });
                }
                break;
            case CommandType.Clear:
                foreach (string item in dto.EntityPaths)
                {
                    string entityName = Path.GetFileNameWithoutExtension(item);

                }
                break;
            default:
                break;
        }
        _codeGenService.GenerateFiles(files);
    }
}
