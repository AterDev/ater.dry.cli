
using Ater.Web.Abstraction;
using DeepSeek.Core;

namespace Application;
/// <summary>
/// AI服务
/// </summary>
public class AIService
{
    private readonly CommandDbContext _dbContext;
    private readonly ILogger<AIService> _logger;
    public DeepSeekClient? Client { get; private set; }

    public AIService(ILogger<AIService> logger)
    {
        _logger = logger;
        _dbContext = WebAppContext.GetScopeService<CommandDbContext>()
            ?? throw new Exception("CommandDBContext is not inject");
    }


    /// <summary>
    /// SetApiKey
    /// </summary>
    /// <param name="key">模型配置key</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public void SetApiKey(string key)
    {
        var apiKey = _dbContext.Configs.Where(c => c.Key == key)
           .Select(c => c.Value)
           .FirstOrDefault() ?? throw new Exception("apiKey is null");

        Client = new DeepSeekClient(apiKey);
    }


    public async Task<string> GetAnswerAsync(string question)
    {
        if (Client == null)
        {
            throw new Exception("Client is null");
        }

        return "";
    }

}
