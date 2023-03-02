using Share.Models.${EntityName}Dtos;

namespace ${Namespace}.IManager;
/// <summary>
/// 定义实体业务接口规范
/// </summary>
public interface I${EntityName}Manager : IDomainManager<${EntityName}, ${EntityName}UpdateDto, ${EntityName}FilterDto, ${EntityName}ItemDto>
{
	/// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<${EntityName}?> GetOwnedAsync(Guid id);

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<${EntityName}> CreateNewEntityAsync(${EntityName}AddDto dto);
}
