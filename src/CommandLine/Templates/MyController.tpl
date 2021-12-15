
namespace {$Namespace}.Controllers
{
    /// <summary>
    /// {$Description}
    /// </summary>
    public class {$EntityName}Controller : ApiController<{$EntityName}Repository, {$EntityName}, {$EntityName}AddDto, {$EntityName}UpdateDto, {$EntityName}Filter, {$EntityName}Dto>
    {
        public {$EntityName}Controller(
            ILogger<{$EntityName}Controller> logger,
            {$EntityName}Repository repository, IUserContext userContext) : base(logger, repository, userContext)
        {
        }

        /// <summary>
        /// 添加{$Description}
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public override async Task<ActionResult<{$EntityName}>> AddAsync([FromBody] {$EntityName}AddDto form)
        {
            // if (_repos.Any(e => e.Name == form.Name))
            // {
            //     return Conflict();
            // }
            return await _repos.AddAsync(form);
        }

        /// <summary>
        /// 分页筛选{$Description}
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("filter")]
        public override async Task<ActionResult<PageResult<{$EntityName}Dto>>> FilterAsync({$EntityName}Filter filter)
        {
            return await _repos.GetListWithPageAsync(filter);
        }

        /// <summary>
        /// 更新{$Description}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public override async Task<ActionResult<{$EntityName}>> UpdateAsync([FromRoute] Guid id, [FromBody] {$EntityName}UpdateDto form)
        {
            if (_repos.Any(e => e.Id == id))
            {
                // 名称不可以修改成其他已经存在的名称
                // if (_repos.Any(e => e.Name == form.Name && e.Id != id))
                // {
                //    return Conflict();
                // }
                return await _repos.UpdateAsync(id, form);
            }
            return NotFound();
        }


        /// <summary>
        /// 删除{$Description}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public override Task<ActionResult<{$EntityName}>> DeleteAsync([FromRoute] Guid id)
        {
            return base.DeleteAsync(id);
        }

        /// <summary>
        /// 获取{$Description}详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public override Task<ActionResult<{$EntityName}>> GetDetailAsync([FromRoute] Guid id)
        {
            return base.GetDetailAsync(id);
        }
    }
}