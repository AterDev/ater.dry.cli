using CodeGenerator;
using CodeGenerator.Helper;
using CodeGenerator.Models;
using Entity;
using Microsoft.Extensions.Logging;
using Share.Services;

namespace Command.Share;

public class CommandRunner(CodeGenService codeGen, CodeAnalysisService codeAnalysis, ILogger<CommandRunner> logger)
{
    private readonly CodeGenService _codeGen = codeGen;
    private readonly CodeAnalysisService _codeAnalysis = codeAnalysis;
    private readonly ILogger<CommandRunner> _logger = logger;

    public void UpdateStudio()
    {

    }

    public async Task RunStudioAsync()
    {
    }

    /// <summary>
    /// angular 代码生成
    /// </summary>
    /// <param name="url">swagger json地址</param>
    /// <param name="output">ng前端根目录</param>
    /// <returns></returns>
    public async Task GenerateNgAsync(string url = "", string output = "")
    {
        try
        {
            _logger.LogInformation("🚀 Generating ts models and ng services...");
            RequestCommand cmd = new(url, output, RequestLibType.NgHttp);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            _logger.LogInformation(webExp.Message);
            _logger.LogInformation("Ensure you had input correct url!");
        }
        catch (Exception exp)
        {
            _logger.LogInformation(exp.Message);
            _logger.LogInformation(exp.StackTrace);
        }
    }
    /// <summary>
    /// 请求服务生成
    /// </summary>
    /// <param name="url"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public async Task GenerateRequestAsync(string url = "", string output = "", RequestLibType type = RequestLibType.NgHttp)
    {
        try
        {
            _logger.LogInformation($"🚀 Generating ts models and {type} request services...");
            RequestCommand cmd = new(url, output, type);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            _logger.LogInformation(webExp.Message);
            _logger.LogInformation("Ensure you had input correct url!");
        }
        catch (Exception exp)
        {
            _logger.LogInformation(exp.Message);
            _logger.LogInformation(exp.StackTrace);
        }
    }

    /// <summary>
    /// dto生成或更新
    /// </summary>
    /// <param name="entityPath"></param>
    public async Task GenerateDtoAsync(string entityPath, string outputPath, bool force)
    {
        var entityInfo = await GetEntityInfoAsync(entityPath);
        var files = _codeGen.GenerateDto(entityInfo, outputPath, force);
        _codeGen.GenerateFiles(files);
    }

    private static async Task<EntityInfo> GetEntityInfoAsync(string entityPath)
    {
        var helper = new EntityParseHelper(entityPath);
        var entityInfo = await helper.ParseEntityAsync();
        _ = entityInfo ?? throw new Exception("实体解析失败，请检查实体文件是否正确！");
        return entityInfo;
    }

    /// <summary>
    /// manager代码生成
    /// </summary>
    /// <param name="entityPath">entity path</param>
    /// <param name="sharePath"></param>
    /// <param name="applicationPath"></param>
    /// <returns></returns>
    public async Task GenerateManagerAsync(string entityPath, string sharePath = "",
            string applicationPath = "", bool force = false)
    {
        var entityInfo = await GetEntityInfoAsync(entityPath);
        var files = new List<GenFileInfo>();

        files.AddRange(_codeGen.GenerateDto(entityInfo, sharePath, force));
        var tplContent = TplContent.GetManagerTpl();
        files.AddRange(_codeGen.GenerateManager(entityInfo, applicationPath, tplContent, force));
        _codeGen.GenerateFiles(files);
    }

    /// <summary>
    /// api项目代码生成
    /// </summary>
    /// <param name="entityPath">实体文件路径</param>
    /// <param name="applicationPath">service目录</param>
    /// <param name="apiPath">网站目录</param>
    /// <param name="suffix">控制器后缀名</param>
    public async Task GenerateApiAsync(string entityPath, string sharePath = "",
            string applicationPath = "", string apiPath = "", bool force = false)
    {
        try
        {
            var entityInfo = await GetEntityInfoAsync(entityPath);
            var files = new List<GenFileInfo>();

            files.AddRange(_codeGen.GenerateDto(entityInfo, sharePath, force));
            var tplContent = TplContent.GetManagerTpl();
            files.AddRange(_codeGen.GenerateManager(entityInfo, applicationPath, tplContent, force));

            tplContent = TplContent.GetControllerTpl();
            var controllerFiles = _codeGen.GenerateController(entityInfo, apiPath, tplContent, force);
            files.AddRange(controllerFiles);
            _codeGen.GenerateFiles(files);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("异常:" + ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }

    /// <summary>
    /// 清除生成代码
    /// </summary>
    /// <param name="EntityName">实体类名称</param>
    public async Task ClearCodesAsync(string entityPath, string sharePath, string applicationPath, string apiPath, string EntityName)
    {
        if (EntityName.ToLower().Equals("systemuser"))
        {
            _logger.LogInformation("⚠️ SystemUser can't be deleted, skip it!");
            return;
        }

        var entityInfo = await GetEntityInfoAsync(entityPath);
        _logger.LogInformation($"start cleaning {EntityName}");
        // 清理dto
        string dtoPath = Path.Combine(sharePath, "Models", EntityName + "Dtos");
        if (Directory.Exists(dtoPath))
        {
            Directory.Delete(dtoPath, true);
            _logger.LogInformation("✔️ clear dtos");
        }

        // 清理data store
        string storePath = Path.Combine(applicationPath, "CommandStore", EntityName + "CommandStore.cs");
        if (File.Exists(storePath))
        {
            File.Delete(storePath);
            _logger.LogInformation("✔️ clear commandstore");
        }
        storePath = Path.Combine(applicationPath, "QueryStore", EntityName + "QueryStore.cs");
        if (File.Exists(storePath))
        {
            File.Delete(storePath);
            _logger.LogInformation("✔️ clear querystore");
        }


        // 清理manager
        string managerPath = Path.Combine(applicationPath, "Manager", EntityName + "Manager.cs");
        if (File.Exists(managerPath))
        {
            File.Delete(managerPath);
            _logger.LogInformation("✔️ clear manager");
        }

        try
        {
            // 更新 依赖注入
            string entityFilePath = Directory.GetFiles(entityPath, EntityName + ".cs", SearchOption.AllDirectories).First();

            var managerDIFile = _codeGen.GetManagerService(entityInfo, applicationPath);
            _codeGen.GenerateFiles([managerDIFile]);

            _logger.LogInformation("✔️ update manager service extention");

            // 清除web api 
            string apiControllerPath = Path.Combine(apiPath, "Controllers");

            List<string> files = Directory.GetFiles(apiControllerPath, $"{EntityName}Controller.cs", SearchOption.AllDirectories).ToList();
            files.ForEach(f =>
            {
                File.Delete(f);
                _logger.LogInformation($"✔️ clear api {f}");
            });

            // 清除test
            string testPath = Path.Combine(apiPath, "..", "..", "test", "Application.Test");
            if (Directory.Exists(testPath))
            {
                string testFile = Path.Combine(testPath, "Managers", $"{EntityName}ManagerTest.cs");
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                    _logger.LogInformation($"✔️ clear test {testFile}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message + ex.InnerException + ex.StackTrace);
        }
    }

    /// <summary>
    /// 生成客户端api client
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="swaggerUrl"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    public static async Task GenerateCSharpApiClientAsync(string swaggerUrl, string outputPath, LanguageType language = LanguageType.CSharp)
    {
        ApiClientCommand cmd = new(swaggerUrl, outputPath, language);
        await cmd.RunAsync();
    }
}


