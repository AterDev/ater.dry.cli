using Share.Models.${EntityName}Dtos;

namespace ${Namespace}.IManager;
/// <summary>
/// 定义实体业务接口规范
/// </summary>
public interface I${EntityName}Manager : IDomainManager<${EntityName}, ${EntityName}UpdateDto, ${EntityName}FilterDto, ${EntityName}ItemDto>
{
	// TODO: 定义业务方法
}
