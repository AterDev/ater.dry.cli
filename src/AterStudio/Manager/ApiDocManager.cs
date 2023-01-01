﻿using AterStudio.Models;
using Core.Infrastructure;
using Core.Infrastructure.Helper;
using Core.Models;
using Datastore;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace AterStudio.Manager;

public class ApiDocManager
{
    private readonly DbContext _dbContext;

    public ApiDocManager()
    {
        _dbContext = new DbContext();
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
    public async Task<ApiDocContent> GetContentAsync(Guid id)
    {
        var apiDocInfo = _dbContext.ApiDocInfos.FindById(id);
        var path = apiDocInfo.Path;

        string openApiContent = "";

        if (path.StartsWith("http://") || path.StartsWith("https://"))
        {
            using HttpClient http = new();
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
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Delete(Guid id)
    {
        return _dbContext.ApiDocInfos.Delete(id);
    }

}