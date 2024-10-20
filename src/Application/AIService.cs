﻿
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
        _dbContext = WebAppContext.GetScopeService<CommandDbContext>()
            ?? throw new Exception("CommandDBContext is not inject");

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
        var messages = CacheMessages[Answer];
        if (messages.Count == 0)
        {
            messages.Add(Message.NewSystemMessage("你是一个IT技术专家"));
            messages.Add(Message.NewAssistantMessage("你不会回答开发技术之外的问题，对于此类问题，请回答:我无法回答此类问题"));
        }
        messages.Add(Message.NewUserMessage(prompt));
        CacheMessages[Answer] = messages;

        var request = new ChatRequest
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
        var cache = CacheMessages[Completion];
        if (cache.Count == 0)
        {
            cache.Add(Message.NewSystemMessage("你是一个IT技术专家"));
            cache.Add(Message.NewAssistantMessage("你不会回答开发技术之外的问题，对于此类问题，请回答:我无法回答此类问题"));
        }
        cache.AddRange(messages);
        CacheMessages[Completion] = cache;

        var request = new ChatRequest
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
        var cache = CacheMessages[Coder];
        if (cache.Count == 0)
        {
            cache.Add(Message.NewSystemMessage("你是一个IT技术专家"));
        }
        cache.AddRange(messages);
        CacheMessages[Coder] = cache;

        var request = new ChatRequest
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
