using AterStudio.Models;
using Core.Entities;
using Core.Infrastructure.Helper;
using Datastore;
using Microsoft.OpenApi.Readers;

namespace AterStudio.Manager;
public class SwaggerManager
{
    private readonly DbContext _dbContext;
    public string? ErrorMsg { get; set; }

    public SwaggerManager(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
                http.Timeout = TimeSpan.FromSeconds(5);
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
        return NgPageGenerate.GenFormComponent(dto.ModelInfo, dto.ServiceName);
    }
}
