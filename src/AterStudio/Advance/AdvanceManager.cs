using System.Collections.Concurrent;
using System.Text;

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

    public AdvanceManager(DbContext dbContext, DusiHttpClient httpClient, ProjectContext projectContext)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _projectContext = projectContext;

        var openAIKey = _dbContext.Configs.Find(c => c.Key == ConfigData.OpenAI).FirstOrDefault();
        if (openAIKey != null)
        {
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
    /// 获取实体表结构
    /// </summary>
    /// <returns>markdown format</returns>
    public string GetDatabaseStructure()
    {
        var res = new ConcurrentBag<string>();
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

            var entityFiles = Directory.GetFiles(Path.Combine(_projectContext.SolutionPath!, Config.EntityPath), $"*.cs", SearchOption.AllDirectories).ToList();

            Parallel.ForEach(
                propertyTypes,
                new ParallelOptions { MaxDegreeOfParallelism = 5 },
                propertyType =>
                {
                    var entityPath = entityFiles.Where(e => e.EndsWith($"{propertyType}.cs")).FirstOrDefault();
                    if (entityPath != null)
                    {
                        var entityCompilation = new EntityParseHelper(entityPath);
                        var entityInfo = entityCompilation.GetEntity();
                        res.Add(ToMarkdown(entityInfo));
                    }
                });
        }
        GC.Collect();
        return string.Join(Environment.NewLine, res);
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