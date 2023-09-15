using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Core;
using Core.Entities;
using Core.Infrastructure.Helper;
using Datastore;

namespace AterStudio.Advance;

public class EntityAdvance
{
    private readonly DbContext _dbContext;
    private readonly DusiHttpClient _httpClient;
    private readonly ProjectContext _projectContext;
    private readonly OpenAIClient? _openAI;

    public EntityAdvance(DbContext dbContext, DusiHttpClient httpClient, ProjectContext projectContext)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _projectContext = projectContext;

        var openAIKey = _dbContext.Configs.Find(c => c.Key == ConfigData.OpenAI).FirstOrDefault();
        if (openAIKey != null)
        {
            _openAI = new OpenAIClient(openAIKey.Value);
        }
    }


    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetConfig(string key, string value)
    {
        var config = _dbContext.Configs.FindOne(c => c.Key == key);
        if (config != null)
        {
            config.Value = value;
            _dbContext.Configs.Update(config);
        }
        else
        {
            config = new ConfigData
            {
                Key = key,
                Value = value
            };
            _dbContext.Configs.Insert(config);
        }
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ConfigData? GetConfig(string key)
    {
        return _dbContext.Configs.FindOne(c => c.Key == key);
    }


    /// <summary>
    /// 生成实体内容
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<Response<StreamingChatCompletions>?> GenerateEntityAsync(string content)
    {
        if (_openAI != null)
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "你是编程助手，为用户提供代码示例"),
                    new ChatMessage(ChatRole.User, content),
                    new ChatMessage(ChatRole.Assistant, "根据上面内容，生成C# 模型类，要求带上注释和相关特性;如果有枚举类型，生成枚举类，并带上注释和Description特性"),
                }
            };

            return await _openAI.GetChatCompletionsStreamingAsync("gpt-3.5-turbo", chatCompletionsOptions);
        }
        return default;
    }


    /// <summary>
    /// 获取实体表结构
    /// </summary>
    /// <param name="id">项目id</param>
    /// <returns>markdown format</returns>
    public string GetDatabaseStructure(Guid id)
    {
        var result = "";
        // get contextbase path 
        var contextPath = Directory.GetFiles(_projectContext.SolutionPath!, "ContextBase.cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (contextPath != null)
        {
            // parse context content and get all proporties 
            var compilation = new CompilationHelper(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath));
            compilation.AddSyntaxTree(File.ReadAllText(contextPath));

            var propertyTypes = compilation.GetPropertyTypes();

            // search entity use propertyType and parse every entity use CompilationHelper
            foreach (var propertyType in propertyTypes)
            {
                var entityPath = Directory.GetFiles(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath), $"{propertyType}.cs", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (entityPath != null)
                {
                    var entityCompilation = new EntityParseHelper(entityPath);
                    var entityInfo = entityCompilation.GetEntity();
                    result += ToMarkdown(entityInfo);

                }
            }
        }
        return result;
    }

    /// <summary>
    /// 实体转换为markdown table
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <returns></returns>
    public string ToMarkdown(EntityInfo entityInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## {entityInfo.Name} ({entityInfo.Summary})");
        sb.AppendLine($"|字段名|类型|是否可空|说明|");
        sb.AppendLine($"|---|---|---|---|");
        foreach (var property in entityInfo.PropertyInfos)
        {
            var comment = string.IsNullOrWhiteSpace(property.CommentSummary)
                ? "无"
                : property.CommentSummary;

            sb.AppendLine($"|{property.Name}|{property.Type}|{property.IsNullable}|{comment}|");
        }
        return sb.ToString();
    }


}
