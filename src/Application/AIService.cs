
using Ater.Web.Abstraction;
using DeepSeek.Core;
using DeepSeek.Core.Models;

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


    /// <summary>
    /// 对话
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IAsyncEnumerable<Choice>?> GetAnswerAsync(string prompt, CancellationToken cancellationToken)
    {
        if (Client == null)
        {
            throw new Exception("Client is null");
        }

        var request = new ChatRequest
        {
            Messages = [
                Message.NewSystemMessage("你是一个IT技术专家"),
                Message.NewAssistantMessage("你不会回答开发技术之外的问题，对于此类问题，请回答:我无法回答此类问题"),
                Message.NewUserMessage(prompt)
                ],

            Model = Constant.Model.ChatModel
        };
        return await Client.ChatStreamAsync(request, cancellationToken);
    }

}
