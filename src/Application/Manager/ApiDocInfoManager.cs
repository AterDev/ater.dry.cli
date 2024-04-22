using System.Text;
using Definition.Share.Models.ApiDocInfoDtos;

namespace Application.Manager;
/// <summary>
/// 接口文档
/// </summary>
public class ApiDocInfoManager(
    DataAccessContext<ApiDocInfo> dataContext,
    ProjectContext project,
    ILogger<ApiDocInfoManager> logger
    ) : ManagerBase<ApiDocInfo, ApiDocInfoUpdateDto, ApiDocInfoFilterDto, ApiDocInfoItemDto>(dataContext, logger)
{
    private readonly ProjectContext _project = project;

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ApiDocInfo> CreateNewEntityAsync(ApiDocInfoAddDto dto)
    {
        var entity = dto.MapTo<ApiDocInfoAddDto, ApiDocInfo>();
        entity.ProjectId = _project.ProjectId;
        return await Task.FromResult(entity);
    }

    public override async Task<ApiDocInfo> UpdateAsync(ApiDocInfo entity, ApiDocInfoUpdateDto dto)
    {
        return await base.UpdateAsync(entity, dto);
    }

    public override async Task<PageList<ApiDocInfoItemDto>> FilterAsync(ApiDocInfoFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.ProjectId, q => q.ProjectId == filter.ProjectId)
            .WhereNotNull(filter.Name, q => q.Name == filter.Name);
        return await Query.FilterAsync<ApiDocInfoItemDto>(Queryable, filter.PageIndex, filter.PageSize, filter.OrderBy);
    }


    /// <summary>
    /// 解析并获取文档内容
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocContent?> GetContentAsync(Guid id)
    {
        var apiDocInfo = await FindAsync(id);
        var path = apiDocInfo!.Path;

        string openApiContent = "";
        try
        {
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };

                using HttpClient http = new(handler);

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

    /// <summary>
    /// 导出文档
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<string> ExportDocAsync(Guid id)
    {
        var apiDocInfo = await FindAsync(id);
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
    /// 是否唯一
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsConflictAsync(string unique)
    {
        // TODO:自定义唯一性验证参数和逻辑
        return await Command.Db.AnyAsync(q => q.Id == new Guid(unique));
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ApiDocInfo?> GetOwnedAsync(Guid id)
    {
        var query = Command.Db.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
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
