using System.Text;
using AterStudio.Models;
using Core;
using Core.Entities;
using Core.Infrastructure.Helper;
using Core.Infrastructure.Utils;
using Datastore;

namespace AterStudio.Advance;

public class EntityAdvance
{
    private readonly DbContext _dbContext;
    private readonly DusiHttpClient _httpClient;
    private readonly ProjectContext _projectContext;

    public EntityAdvance(DbContext dbContext, DusiHttpClient httpClient, ProjectContext projectContext)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _projectContext = projectContext;
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

    public async Task<string?> GetTokenAsync(string username, string password)
    {
        return await _httpClient.GetTokenAsync(username, password);
    }

    public async Task<List<string>?> GetEntityAsync(string name, string description, string token)
    {
        _httpClient.SetToken(token);
        return await _httpClient.GetEntityAsync(name, description);
    }

    /// <summary>
    /// 创建新的实体
    /// </summary>
    /// <param name="dto"></param>
    public bool CreateEntity(AddEntityDto dto)
    {
        string entityPath = Path.Combine(_projectContext.EntityPath!, "Entities");
        string namespaceContent = "namespace Core.Entities";
        if (!string.IsNullOrWhiteSpace(dto.Namespace))
        {
            entityPath = Path.Combine(entityPath, dto.Namespace);
            namespaceContent += $".{dto.Namespace.ToPascalCase()}";
        }
        if (!Directory.Exists(entityPath))
        {
            Directory.CreateDirectory(entityPath);
        }
        namespaceContent = namespaceContent + ";" + Environment.NewLine;
        // 预处理
        var code = dto.Content.Replace("[Requried]", "");
        code = namespaceContent + code;

        // 代码分析
        var compilation = new CompilationHelper(entityPath);
        compilation.AddSyntaxTree(code);

        // 继承EntityBase
        if (compilation.GetParentClassName() == null)
        {
            compilation.AddClassBaseType("EntityBase");
        }
        try
        {
            var interfaceContent = compilation.SyntaxRoot!.ToString();
            File.WriteAllTextAsync(Path.Combine(entityPath, compilation.ClassSymbol!.Name + ".cs"), interfaceContent);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
            return false;
        }
    }
}
