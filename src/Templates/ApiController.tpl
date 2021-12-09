// 该控制器由GT.CLI自动生成,继承该类,可以实现基本的CURD Api接口,以及用户相关信息
// 可以直接继承ApiServiceBase基类,或实现IApiServiceBase接口,以实现自定义的http路由及方法

namespace App.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiExplorerSettings(GroupName = "")]
    //[Authorize("Admin")]
    public class ApiController<TRepository, TEntity, TAddForm, TUpdateForm, TFilter, TDto>
        : ApiControllerBase<{$ContextName}, TRepository, TEntity, TAddForm, TUpdateForm, TFilter, TDto>
        where TRepository : RepositoryBase<{$ContextName}, TEntity, TAddForm, TUpdateForm, TFilter, TDto, Guid>
        where TFilter : FilterBase
        where TEntity : BaseDB
    
        IUserContext _userCtx;
        public ApiController(ILogger logger, TRepository repos, IUserContext userContext) : base(logger, repos)
        {
              _userCtx = userContext;
        }

         // 自定义逻辑及方法
    }
}
