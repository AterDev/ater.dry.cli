using Microsoft.SemanticKernel;

namespace Application;
/// <summary>
/// AI服务
/// </summary>
public class AIService(CommandDbContext dbContext)
{
    public Kernel? Kernel { get; private set; }
    private readonly CommandDbContext _dbContext = dbContext;

    /// <summary>
    /// build kernel
    /// </summary>
    /// <param name="key">模型配置key</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Kernel BuildKernel(string key)
    {
        var apiKey = _dbContext.Configs.Where(c => c.Key == key)
           .Select(c => c.Value)
           .FirstOrDefault();

        var builder = Kernel.CreateBuilder();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("deepSeekApiKey is not set");
        }

#pragma warning disable SKEXP0010
        builder.AddOpenAIChatCompletion("deepseek-chat", new Uri("https://api.deepseek.com/"), apiKey);

        Kernel = builder.Build();
        return Kernel;
    }



    public async Task<string> TestAsync(string prompt)
    {
        var results = Kernel.InvokePromptStreamingAsync(prompt);

        await foreach (var result in results)
        {
            Console.Write(".");
        }

        Console.WriteLine("done");

        return "";
    }
}
