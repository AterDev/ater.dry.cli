using System.Text;

using Azure;
using Azure.AI.OpenAI;

using Core;
using Core.Entities;
using Core.Infrastructure.Helper;

using Datastore;

namespace AterStudio.Advance;

public class AdvanceManager
{
    private readonly DbContext _dbContext;
    private readonly DusiHttpClient _httpClient;
    private readonly ProjectContext _projectContext;
    private readonly OpenAIClient? _openAI;

    public AdvanceManager(DbContext dbContext, DusiHttpClient httpClient, ProjectContext projectContext)
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
                    new ChatMessage(ChatRole.System, "你是一个严谨完善的编程助手，为用户提供代码示例"),
                    new ChatMessage(ChatRole.User, content),
                    new ChatMessage(ChatRole.User, "根据上面内容，生成C#实体模型类，为类和属性添加注释和特性;如果有枚举类型，生成枚举类，并带上注释和Description特性。注释使用C#风格的XML注释，只需要提供模型类即可。"),
                }
            };

            return await _openAI.GetChatCompletionsStreamingAsync("gpt-3.5-turbo", chatCompletionsOptions);
        }
        return default;
    }

    /// <summary>
    /// 问答
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<Response<StreamingChatCompletions>?> GenerateAnswerAsync(string content)
    {
        if (_openAI != null)
        {
            var prompts = new List<string>()
            {
                content
            };

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                Messages =
                {
                    new ChatMessage(ChatRole.User, content),
                }
            };

            return await _openAI.GetChatCompletionsStreamingAsync("gpt-3.5-turbo", chatCompletionsOptions);
        }
        return default;
    }


    /// <summary>
    /// 生成图片
    /// </summary>
    /// <param name="content"></param>
    /// <param name="size"></param>
    public async Task<List<string>?> GenerateImagesAsync(string content, ImageSize size = ImageSize.Middle)
    {
        if (_openAI != null)
        {
            var imageOptions = new ImageGenerationOptions()
            {
                Prompt = content,
                ImageCount = 1,

            };
            imageOptions.Size = size switch
            {
                ImageSize.Small => Azure.AI.OpenAI.ImageSize.Size256x256,
                ImageSize.Middle => Azure.AI.OpenAI.ImageSize.Size512x512,
                ImageSize.Large => Azure.AI.OpenAI.ImageSize.Size1024x1024,
                _ => Azure.AI.OpenAI.ImageSize.Size512x512
            };

            var response = await _openAI.GetImageGenerationsAsync(imageOptions);
            if (response != null)
            {
                var images = response.Value.Data.ToList();
                return images.Select(image => image.Url.ToString()).ToList();
            }

        }
        return default;
    }

    public void UploadFile(Stream stream)
    {
        // use _openAI to upload file 
        if (_openAI != null)
        {

        }
    }


    /// <summary>
    /// 获取实体表结构
    /// </summary>
    /// <returns>markdown format</returns>
    public string GetDatabaseStructure()
    {

        var sb = new StringBuilder();
        // get contextbase path 
        var contextPath = Directory.GetFiles(_projectContext.SolutionPath!, "ContextBase.cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (contextPath != null)
        {
            // parse context content and get all properties 
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
                    sb.AppendLine(ToMarkdown(entityInfo));
                }
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 实体转换为 markdown table
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


public enum ImageSize
{
    /// <summary>
    /// 256*256
    /// </summary>
    Small,
    /// <summary>
    /// 512*512
    /// </summary>
    Middle,
    /// <summary>
    /// 1024*1024
    /// </summary>
    Large
}