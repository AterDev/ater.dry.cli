using System.Text;
using AterStudio.Models;
using Core.Entities;
using Core.Infrastructure.Helper;
using Datastore;
using Microsoft.OpenApi.Readers;

namespace AterStudio.Manager;
public class SwaggerManager(DbContext dbContext)
{
    private readonly DbContext _dbContext = dbContext;
    public string? ErrorMsg { get; set; }

    public ApiDocInfo? Find(Guid id)
    {
        return _dbContext.ApiDocInfos.FindById(id);
    }

    public List<ApiDocInfo> FindAll(Project project)
    {
        return _dbContext.ApiDocInfos.Query()
            .Where(d => d.ProjectId == project.Id)
            .ToList();
    }

    /// <summary>
    /// 解析并获取文档内容
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocContent?> GetContentAsync(Guid id)
    {
        var apiDocInfo = _dbContext.ApiDocInfos.FindById(id);
        var path = apiDocInfo.Path;

        string openApiContent = "";
        try
        {
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                using HttpClient http = new();
                http.Timeout = TimeSpan.FromSeconds(60);
                openApiContent = await http.GetStringAsync(path);
            }
            else
            {
                openApiContent = File.ReadAllText(path);
            }
            openApiContent = openApiContent
                .Replace("«", "")
                .Replace("»", "");

            var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);
            var helper = new OpenApiHelper(apiDocument);

            return new ApiDocContent
            {
                ModelInfos = helper.ModelInfos,
                OpenApiTags = helper.OpenApiTags,
                RestApiGroups = helper.RestApiGroups
            };
        }
        catch (Exception ex)
        {
            ErrorMsg = $"{path} 请求失败！" + ex.Message;
            return default;
        }
    }
    public async Task<string> ExportDocAsync(Guid id)
    {
        var apiDocInfo = _dbContext.ApiDocInfos.FindById(id);
        var path = apiDocInfo.Path;
        string openApiContent = "";
        if (path.StartsWith("http://") || path.StartsWith("https://"))
        {
            using HttpClient http = new();
            http.Timeout = TimeSpan.FromSeconds(60);
            openApiContent = await http.GetStringAsync(path);
        }
        else
        {
            openApiContent = File.ReadAllText(path);
        }
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);
        var helper = new OpenApiHelper(apiDocument);
        var groups = helper.RestApiGroups;

        var sb = new StringBuilder();
        foreach (var group in groups)
        {
            sb.AppendLine("## " + group.Description);
            sb.AppendLine();
            foreach (var api in group.ApiInfos)
            {
                sb.AppendLine($"### {api.Summary}");
                sb.AppendLine();
                sb.AppendLine("|||");
                sb.AppendLine("|---------|---------|");
                sb.AppendLine($"|接口说明|{api.Summary}|");
                sb.AppendLine($"|接口地址|{api.Router}|");
                sb.AppendLine($"|接口方法|{api.OperationType.ToString()}|");
                sb.AppendLine();

                sb.AppendLine("#### 请求内容");
                sb.AppendLine();
                var requestInfo = api.RequestInfo;
                if (requestInfo != null)
                {

                    sb.AppendLine("|名称|类型|是否必须|说明|");
                    sb.AppendLine("|---------|---------|---------|---------|");
                    foreach (var property in requestInfo.PropertyInfos)
                    {
                        sb.AppendLine($"|{property.Name}|{property.Type}|{(property.IsRequired ? "是" : "否")}|{property.CommentSummary?.Trim()}|");
                    }
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("无");
                }
                sb.AppendLine();
                sb.AppendLine("#### 返回内容");
                var responseInfo = api.ResponseInfo;
                if (responseInfo != null)
                {
                    sb.AppendLine("|名称|类型|说明|");
                    sb.AppendLine("|---------|---------|---------|");
                    foreach (var property in responseInfo.PropertyInfos)
                    {
                        sb.AppendLine($"|{property.Name}|{property.Type}|{property.CommentSummary?.Trim()}|");
                    }
                }
                else
                {
                    sb.AppendLine("无");
                }
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="apiDocInfo"></param>
    /// <returns></returns>
    public ApiDocInfo AddApiDoc(ApiDocInfo apiDocInfo)
    {
        _dbContext.ApiDocInfos.Insert(apiDocInfo);
        return apiDocInfo;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="apiDocInfo"></param>
    /// <returns></returns>
    public ApiDocInfo? UpdateApiDoc(Guid id, ApiDocInfo apiDocInfo)
    {
        var doc = _dbContext.ApiDocInfos.FindById(id);
        if (doc != null)
        {
            doc.Description = apiDocInfo.Description;
            doc.Path = apiDocInfo.Path;
            doc.Name = apiDocInfo.Name;
            _dbContext.ApiDocInfos.Update(doc);
        }
        return doc;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Delete(Guid id)
    {
        return _dbContext.ApiDocInfos.Delete(id);
    }

    /// <summary>
    /// 生成页面组件
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public NgComponentInfo CreateUIComponent(CreateUIComponentDto dto)
    {
        return dto.ComponentType switch
        {
            ComponentType.Form => NgPageGenerate.GenFormComponent(dto.ModelInfo, dto.ServiceName),
            ComponentType.Table => NgPageGenerate.GenTableComponent(dto.ModelInfo, dto.ServiceName),
            _ => default!,
        };
    }
}
