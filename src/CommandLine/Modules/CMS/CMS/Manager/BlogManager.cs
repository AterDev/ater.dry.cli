using Application.Implement;
using Application.IManager;
using Share.Models.BlogDtos;

namespace Application.Manager;
/// <summary>
/// 博客
/// </summary>
public class BlogManager : DomainManagerBase<Blog, BlogUpdateDto, BlogFilterDto, BlogItemDto>, IBlogManager
{

    public BlogManager(
        DataStoreContext storeContext, 
        ILogger<BlogManager> logger,
        IUserContext userContext) : base(storeContext, logger)
    {

        _userContext = userContext;
    }

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Blog> CreateNewEntityAsync(BlogAddDto dto)
    {
        var entity = dto.MapTo<BlogAddDto, Blog>();
        Command.Db.Entry(entity).Property("UserId").CurrentValue = _userContext.UserId!.Value;
        // or entity.UserId = _userContext.UserId!.Value;
        Command.Db.Entry(entity).Property("CatalogId").CurrentValue = dto.CatalogId;
        // or entity.CatalogId = dto.CatalogId;
        // other required props
        return await Task.FromResult(entity);
    }

    public override async Task<Blog> UpdateAsync(Blog entity, BlogUpdateDto dto)
    {
        return await base.UpdateAsync(entity, dto);
    }

    public override async Task<PageList<BlogItemDto>> FilterAsync(BlogFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Title, q => q.Title == filter.Title)
            .WhereNotNull(filter.LanguageType, q => q.LanguageType == filter.LanguageType)
            .WhereNotNull(filter.BlogType, q => q.BlogType == filter.BlogType)
            .WhereNotNull(filter.IsAudit, q => q.IsAudit == filter.IsAudit)
            .WhereNotNull(filter.IsPublic, q => q.IsPublic == filter.IsPublic)
            .WhereNotNull(filter.IsOriginal, q => q.IsOriginal == filter.IsOriginal)
            .WhereNotNull(filter.UserId, q => q.User.Id == filter.UserId)
            .WhereNotNull(filter.CatalogId, q => q.Catalog.Id == filter.CatalogId);
        // TODO: custom filter conditions
        return await Query.FilterAsync<BlogItemDto>(Queryable, filter.PageIndex, filter.PageSize, filter.OrderBy);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Blog?> GetOwnedAsync(Guid id)
    {
        var query = Command.Db.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
