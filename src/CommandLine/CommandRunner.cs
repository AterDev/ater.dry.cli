namespace Droplet.CommandLine;

public class CommandRunner
{
    public CommandRunner()
    {
    }

    public static async Task GenerateDocAsync(string url = "", string output = "")
    {
        try
        {
            Console.WriteLine("🔵 Generating markdown doc");
            var cmd = new DocCommand(url, output);
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
    public static async Task GenerateTSAsync(string url = "", string output = "")
    {
        try
        {
            Console.WriteLine("🔵 Generating ts models");
            var cmd = new TSCommand(url, output);
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
            var cmd = new NgCommand(url, output);
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
        var cmd = new DtoCommand(entityPath, output);
        await cmd.RunAsync(force);
    }

    /// <summary>
    /// api项目代码生成
    /// </summary>
    /// <param name="path">实体文件路径</param>
    /// <param name="servicePath">service目录</param>
    /// <param name="apiPath">网站目录</param>
    /// <param name="dbContext"></param>
    public static async Task GenerateApi(string path, string dtoPath = "",
            string servicePath = "", string apiPath = "", string dbContext = "")
    {
        Console.WriteLine("🔵 Generate dtos");
        var dtoCmd = new DtoCommand(path, dtoPath);
        await dtoCmd.RunAsync();
        Console.WriteLine("🔵 Generate store");
        var storeCmd = new StoreCommand(path, dtoPath, servicePath, dbContext);
        await storeCmd.RunAsync();

        Console.WriteLine("🔵 Generate rest api");
        var apiCmd = new ApiCommand(path, dtoPath, servicePath, apiPath, dbContext);
        await apiCmd.RunAsync();
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
        var viewCmd = new ViewCommand(name, dtoPath, output);
        await viewCmd.RunAsync();
    }
}


