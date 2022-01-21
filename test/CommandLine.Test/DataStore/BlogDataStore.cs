using CommandLine.Test.Models.BlogDtos;
namespace CommandLine.Test.DataStore;
public class BlogDataStore : DataStoreBase<ContextBase, Blog, BlogUpdateDto, BlogFilter, BlogItemDto>
{
    public BlogDataStore(ContextBase context, IUserContext userContext, ILogger<BlogDataStore> logger) : base(context, userContext, logger)
    {
    }
    public override Task<List<BlogItemDto>> FindAsync(BlogFilter filter)
    {
        return base.FindAsync(filter);
    }

    public override Task<PageResult<BlogItemDto>> FindWithPageAsync(BlogFilter filter)
    {
        return base.FindWithPageAsync(filter);
    }
    public override Task<Blog> AddAsync(Blog data) => base.AddAsync(data);
    public override Task<Blog?> UpdateAsync(Guid id, BlogUpdateDto dto) => base.UpdateAsync(id, dto);
    public override Task<bool> DeleteAsync(Guid id) => base.DeleteAsync(id);
}
