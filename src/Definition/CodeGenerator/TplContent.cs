namespace CodeGenerator;
/// <summary>
/// 模板内容类
/// </summary>
public class TplContent
{
    public static string GetManagerTpl()
    {
        return """
            using @(Model.ShareNamespace).Models.@(Model.EntityName)Dtos;

            namespace @(Model.Namespace).Manager;
            @Model.Comment
            public class @(Model.EntityName)Manager(
                DataAccessContext<@(Model.EntityName)> dataContext, 
                ILogger<@(Model.EntityName)Manager> logger,
                IUserContext userContext) : ManagerBase<@(Model.EntityName)>(dataContext, logger)
            {
                private readonly IUserContext _userContext = userContext;

                /// <summary>
                /// 添加实体
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<Guid?> CreateNewEntityAsync(@(Model.EntityName)AddDto dto)
                {
                    var entity = dto.MapTo<@(Model.EntityName)AddDto, @(Model.EntityName)>();
                    // TODO:完善添加逻辑
                    return await base.AddAsync(entity) ? entity.Id : null;
                }

                /// <summary>
                /// 更新实体
                /// </summary>
                /// <param name="entity"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<bool> UpdateAsync(@(Model.EntityName) entity, @(Model.EntityName)UpdateDto dto)
                {
                    entity.Merge(dto);
                    // TODO:完善更新逻辑
                    return await base.UpdateAsync(entity);
                }

                public async Task<PageList<@(Model.EntityName)ItemDto>> ToPageAsync(@(Model.EntityName)FilterDto filter)
                {
            @Model.FilterCode
                }

                /// <summary>
                /// 获取实体详情
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)DetailDto?> GetDetailAsync(Guid id)
                {
                    return await FindAsync<@(Model.EntityName)DetailDto>(e => e.Id == id);
                }

                /// <summary>
                /// TODO:唯一性判断
                /// </summary>
                /// <param name="unique">唯一标识</param>
                /// <param name="id">排除当前</param>
                /// <returns></returns>
                public async Task<bool> IsUniqueAsync(string unique, Guid? id = null)
                {
                    // 自定义唯一性验证参数和逻辑
                    return await Command.Where(q => q.Id.ToString() == unique)
                        .WhereNotNull(id, q => q.Id != id)
                        .AnyAsync();
                }

                /// <summary>
                /// 删除实体
                /// </summary>
                /// <param name="ids"></param>
                /// <param name="softDelete"></param>
                /// <returns></returns>
                public new async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
                {
                    return await base.DeleteAsync(ids, softDelete);
                }

                /// <summary>
                /// 数据权限验证
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)?> GetOwnedAsync(Guid id)
                {
                    var query = Command.Where(q => q.Id == id);
                    // TODO:自定义数据权限验证
                    // query = query.Where(q => q.User.Id == _userContext.UserId);
                    return await query.FirstOrDefaultAsync();
                }
            }
            """;
    }

    /// <summary>
    /// 获取服务注入扩展模板
    /// </summary>
    /// <param name="isModule"></param>
    /// <returns></returns>
    public static string GetManagerServiceExtensionTpl(bool isModule = false)
    {
        return isModule ?
            """
            using @(Model.Namespace).Manager;

            namespace @(Model.Namespace);
            /// <summary>
            /// 服务注入扩展
            /// </summary>
            public static class ServiceCollectionExtensions
            {
                /// <summary>
                /// 添加模块服务
                /// </summary>
                /// <param name="services"></param>
                /// <returns></returns>
                public static IServiceCollection Add@(Model.Namespace)Services(this IServiceCollection services)
                {
                    services.Add@(Model.Namespace)Managers();
                    // add other services
                    return services;
                }


                /// <summary>
                /// 添加@(Model.Namespace) 注入服务
                /// </summary>
                /// <param name="services"></param>
                public static IServiceCollection Add@(Model.Namespace)Managers(this IServiceCollection services)
                {
            @Model.ManagerServices
                    return services;
                }
            }
            """ :

            """
             namespace @(Model.Namespace);

            public static partial class ManagerServiceCollectionExtensions
            {
                public static void AddManager(this IServiceCollection services)
                {
                    services.AddScoped(typeof(DataAccessContext<>));
            @Model.ManagerServices
                }
            }
            """;

    }


    public static string GetControllerTpl()
    {
        return """
            using @(Model.ShareNamespace).Models.@(Model.EntityName)Dtos;
            namespace @(Model.Namespace).Controllers;

            #@Comment#
            public class @(Model.EntityName)Controller(
                IUserContext user,
                ILogger<@(Model.EntityName)Controller> logger,
                @(Model.EntityName)Manager manager
                ) : RestControllerBase<@(Model.EntityName)Manager>(manager, user, logger)
            {
                /// <summary>
                /// 分页数据 🛑
                /// </summary>
                /// <param name="filter"></param>
                /// <returns></returns>
                [HttpPost("filter")]
                public async Task<ActionResult<PageList<@(Model.EntityName)ItemDto>>> FilterAsync(@(Model.EntityName)FilterDto filter)
                {
                    return await _manager.FilterAsync(filter);
                }

                /// <summary>
                /// 新增 🛑
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPost]
                public async Task<ActionResult<Guid?>> AddAsync(@(Model.EntityName)AddDto dto)
                {
                    // 冲突验证
                    // if(await _manager.IsUniqueAsync(dto.xxx)) { return Conflict(ErrorMsg.ConflictResource); }
                    var id = await _manager.AddAsync(dto);
                    return id == null ? Problem(ErrorMsg.AddFailed) : id;
                }

                /// <summary>
                /// 更新数据 🛑
                /// </summary>
                /// <param name="id"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPatch("{id}")]
                public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, @(Model.EntityName)UpdateDto dto)
                {
                    var entity = await _manager.GetOwnedAsync(id);
                    if (entity == null) { return NotFound(ErrorMsg.NotFoundResource); }
                    // 冲突验证
                    return await _manager.UpdateAsync(entity, dto);
                }

                /// <summary>
                /// 获取详情 🛑
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpGet("{id}")]
                public async Task<ActionResult<@(Model.EntityName)?>> GetDetailAsync([FromRoute] Guid id)
                {
                    var res = await _manager.GetDetailAsync(id);
                    return (res == null) ? NotFound() : res;
                }

                /// <summary>
                /// 删除 🛑
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpDelete("{id}")]
                [NonAction]
                public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
                {
                    // 注意删除权限
                    var entity = await _manager.GetOwnedAsync(id);
                    if (entity == null) { return NotFound(); };
                    // return Forbid();
                    return await _manager.DeleteAsync([id], true);
                }
            }
            """;
    }
}
