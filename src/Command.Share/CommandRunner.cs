using Datastore;

namespace Command.Share;

public static class CommandRunner
{
    /// <summary>
    /// 全局静态唯一
    /// </summary>
    public static DbContext dbContext = new DbContext();

    public static async Task GenerateDocAsync(string url = "", string output = "")
    {
        try
        {
            Console.WriteLine("🔵 Generating markdown doc");
            DocCommand cmd = new(url, output);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            Console.WriteLine(webExp.Message);
            Console.WriteLine("Check the url!");
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            Console.WriteLine(exp.StackTrace);
        }
    }
    /// <summary>
    /// angular 代码生成
    /// </summary>
    /// <param name="url">swagger json地址</param>
    /// <param name="output">ng前端根目录</param>
    /// <returns></returns>
    public static async Task GenerateNgAsync(string url = "", string output = "")
    {
        try
        {
            Console.WriteLine("🔵 Generating ts models and ng services...");
            var cmd = new RequestCommand(url, output, RequestLibType.NgHttp);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            Console.WriteLine(webExp.Message);
            Console.WriteLine("Ensure you had input correct url!");
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            Console.WriteLine(exp.StackTrace);
        }
    }
    /// <summary>
    /// 请求服务生成
    /// </summary>
    /// <param name="url"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static async Task GenerateRequestAsync(string url = "", string output = "", RequestLibType type = RequestLibType.NgHttp)
    {
        try
        {
            Console.WriteLine($"🔵 Generating ts models and {type} request services...");
            RequestCommand cmd = new(url, output, type);
            await cmd.RunAsync();
        }
        catch (WebException webExp)
        {
            Console.WriteLine(webExp.Message);
            Console.WriteLine("Ensure you had input correct url!");
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            Console.WriteLine(exp.StackTrace);
        }
    }

    /// <summary>
    /// dto生成或更新
    /// </summary>
    /// <param name="entityPath"></param>
    public static async Task GenerateDtoAsync(string entityPath, string output, bool force)
    {
        Console.WriteLine("🔵 Generating Dtos...");
        DtoCommand cmd = new(entityPath, output);
        await cmd.RunAsync(force);
    }

    /// <summary>
    /// manager代码生成
    /// </summary>
    /// <param name="path">entity path</param>
    /// <param name="dtoPath"></param>
    /// <param name="servicePath"></param>
    /// <returns></returns>
    public static async Task GenerateManagerAsync(string path, string dtoPath = "",
            string servicePath = "", bool force = false)
    {
        Console.WriteLine("🔵 Generate dtos");
        DtoCommand dtoCmd = new(path, dtoPath);
        await dtoCmd.RunAsync(force);
        Console.WriteLine("🔵 Generate manager");
        StoreCommand storeCmd = new(path, dtoPath, servicePath);
        await storeCmd.RunAsync(force);
    }

    /// <summary>
    /// api项目代码生成
    /// </summary>
    /// <param name="path">实体文件路径</param>
    /// <param name="servicePath">service目录</param>
    /// <param name="apiPath">网站目录</param>
    /// <param name="suffix">控制器后缀名</param>
    public static async Task GenerateApiAsync(string path, string dtoPath = "",
            string servicePath = "", string apiPath = "", string suffix = "",bool force = false)
    {
        try
        {
            Console.WriteLine("🔵 Generate dtos");
            DtoCommand dtoCmd = new(path, dtoPath);
            await dtoCmd.RunAsync(force);
            Console.WriteLine("🔵 Generate store");
            StoreCommand storeCmd = new(path, dtoPath, servicePath, suffix);
            await storeCmd.RunAsync(force);

            Console.WriteLine("🔵 Generate rest api");
            ApiCommand apiCmd = new(path, dtoPath, servicePath, apiPath, suffix);
            await apiCmd.RunAsync(force);
        }
        catch (Exception ex)
        {
            Console.WriteLine("异常:" + ex.Message + Environment.NewLine + ex.StackTrace);
        }

    }

    /// <summary>
    /// 根据已生成的dto生成相应的前端表单页面
    /// </summary>
    /// <param name="dtoPath">service根目录</param>
    /// <param name="name">实体类名称</param>
    /// <param name="output">前端根目录</param>
    public static async Task GenerateNgPagesAsync(string name, string dtoPath, string output = "")
    {
        Console.WriteLine("🔵 Generate view");
        ViewCommand viewCmd = new(name, dtoPath, output);
        await viewCmd.RunAsync();
    }

    public static async Task SyncToAngularAsync(string swaggerPath, string entityPath, string dtoPath, string httpPath)
    {
        AutoSyncNgCommand cmd = new(swaggerPath, entityPath, dtoPath, httpPath);
        await cmd.RunAsync();
    }

    /// <summary>
    /// 生成protobuf
    /// </summary>
    /// <param name="entityPath"></param>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public static async Task<string?> GenerateProtobufAsync(string entityPath, string projectPath)
    {
        var cmd = new ProtoCommand(entityPath, projectPath);
        await cmd.RunAsync();
        return cmd.ErrorMessage;
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
        var cmd = new ApiClientCommand(swaggerUrl, outputPath, language);
        await cmd.RunAsync();
    }
}


