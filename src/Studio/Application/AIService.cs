using DeepSeek.Core;
using DeepSeek.Core.Models;

namespace Application;
/// <summary>
/// AI服务
/// </summary>
public class AIService
{
    private readonly ILogger<AIService> _logger;
    public DeepSeekClient? Client { get; private set; }

    public const string Answer = "answer";
    public const string Completion = "completion";
    public const string Coder = "coder";

    /// <summary>
    /// 缓存对话
    /// </summary>
    public Dictionary<string, List<Message>> CacheMessages { get; private set; } = [];

    public AIService(ILogger<AIService> logger)
    {
        _logger = logger;

        CacheMessages.Add(Answer, []);
        CacheMessages.Add(Completion, []);
        CacheMessages.Add(Coder, []);
    }

    /// <summary>
    /// SetApiKey
    /// </summary>
    /// <param name="key">模型配置key</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public void SetApiKey(string apiKey)
    {
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
        List<Message> messages = CacheMessages[Answer];
        if (messages.Count == 0)
        {
            messages.Add(Message.NewSystemMessage("你是一个IT技术专家"));
            messages.Add(Message.NewAssistantMessage("你不会回答开发技术之外的问题，对于此类问题，请回答:我无法回答此类问题"));
        }
        messages.Add(Message.NewUserMessage(prompt));
        CacheMessages[Answer] = messages;

        ChatRequest request = new()
        {
            Messages = messages,
            Model = Constant.Model.ChatModel
        };
        return await Client.ChatStreamAsync(request, cancellationToken);
    }


    /// <summary>
    /// completion
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IAsyncEnumerable<Choice>?> CompletionAsync(List<Message> messages, CancellationToken cancellationToken)
    {
        if (Client == null)
        {
            throw new Exception("Client is null");
        }
        List<Message> cache = CacheMessages[Completion];
        if (cache.Count == 0)
        {
            cache.Add(Message.NewSystemMessage("你是一个IT技术专家"));
            cache.Add(Message.NewAssistantMessage("你不会回答开发技术之外的问题，对于此类问题，请回答:我无法回答此类问题"));
        }
        cache.AddRange(messages);
        CacheMessages[Completion] = cache;

        ChatRequest request = new()
        {
            Messages = cache,
            Model = Constant.Model.ChatModel
        };
        return await Client.ChatStreamAsync(request, cancellationToken);
    }

    /// <summary>
    /// code
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IAsyncEnumerable<Choice>?> CodeAsync(List<Message> messages, CancellationToken cancellationToken)
    {
        if (Client == null)
        {
            throw new Exception("Client is null");
        }
        List<Message> cache = CacheMessages[Coder];
        if (cache.Count == 0)
        {
            cache.Add(Message.NewSystemMessage("你是一个IT技术专家"));
        }
        cache.AddRange(messages);
        CacheMessages[Coder] = cache;

        ChatRequest request = new()
        {
            Messages = cache,
            Model = Constant.Model.CoderModel
        };
        return await Client.ChatStreamAsync(request, cancellationToken);
    }


    public void ClearCache()
    {
        CacheMessages.Clear();
    }

}
