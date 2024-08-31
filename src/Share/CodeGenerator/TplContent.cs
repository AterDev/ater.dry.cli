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
                    return await base.AddAsync(entity) ? entity.Id : null;
                }

                /// <summary>
                /// 更新实体
                /// </summary>
                /// <param name="entity"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)> UpdateAsync(@(Model.EntityName) entity, @(Model.EntityName)UpdateDto dto)
                {
                    entity.Merge(dto);
                    return await base.UpdateAsync(entity);
                }

                public async Task<PageList<@(Model.EntityName)ItemDto>> ToPageAsync(@(Model.EntityName)FilterDto filter)
                {
            @Model.FilterCode
                }


                /// <summary>
                /// 唯一性判断
                /// </summary>
                /// <returns></returns>
                public async Task<bool> IsUniqueAsync(string unique)
                {
                    // TODO:自定义唯一性验证参数和逻辑
                    return await Command.AnyAsync(q => q.Id == new Guid(unique));
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
                /// 当前用户所拥有的对象
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)?> GetOwnedAsync(Guid id)
                {
                    var query = Command.Where(q => q.Id == id);
                    // 获取用户所属的对象
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
    public static string GetManagerServiceExtensionTpl(bool isModule == false)
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
}
