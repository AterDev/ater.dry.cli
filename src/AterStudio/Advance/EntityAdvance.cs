using System.Reflection;
using System.Text;
using AterStudio.Manager;
using Core.Entities;
using Core.Infrastructure.Helper;
using Datastore;

namespace AterStudio.Advance;

public class EntityAdvance
{
    private readonly DbContext _dbContext;
    private readonly DusiHttpClient _httpClient;
    public EntityAdvance(DbContext dbContext, DusiHttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
    }


    /// <summary>
    /// 获取实体表结构
    /// </summary>
    /// <param name="id">项目id</param>
    /// <returns>markdown format</returns>
    public string GetDatabaseStructure(Guid id)
    {
        var result = "";
        var project = _dbContext.Projects.FindById(id);

        var projectPath = ProjectManager.GetProjectRootPath(project.Path);
        // get contextbase path 
        var contextPath = Directory.GetFiles(projectPath, "ContextBase.cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (contextPath != null)
        {
            // parse context content and get all proporties 
            var compilation = new CompilationHelper(project.EntityFrameworkPath);
            compilation.AddSyntaxTree(File.ReadAllText(contextPath));

            var propertyTypes = compilation.GetPropertyTypes();

            // search entity use propertyType and parse every entity use CompilationHelper
            foreach (var propertyType in propertyTypes)
            {
                var entityPath = Directory.GetFiles(project.EntityPath, $"{propertyType}.cs", SearchOption.AllDirectories)
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


    public async Task<string?> GetTokenAsync(string username, string password)
    {
        return await _httpClient.GetTokenAsync(username, password);
    }

    public async Task<List<string>?> GetEntityAsync(string name, string description, string token)
    {
        _httpClient.SetToken(token);
        return await _httpClient.GetEntityAsync(name, description);
    }
}
