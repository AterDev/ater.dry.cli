using System.ComponentModel;

using CodeGenerator.Generate;

using Share.Models;

namespace Share.Services;
/// <summary>
/// 代码生成服务
/// </summary>
public class CodeGenService
{
    public CodeGenService()
    {

    }

    /// <summary>
    /// 生成Dto
    /// </summary>
    /// <param name="entityInfo">实体信息</param>
    /// <param name="outputPath"></param>
    /// <param name="isCover">是否覆盖</param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateDto(EntityInfo entityInfo, string outputPath, bool isCover = false, string? moduleName = null)
    {
        // 生成Dto
        var dtoGen = new DtoCodeGenerate(entityInfo, outputPath);
        var dirName = entityInfo.Name + "Dtos";
        return
        [
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetAddDto(),
                Path = Path.Combine(outputPath, dirName, $"{entityInfo.Name}AddDto.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetUpdateDto(),
                Path = Path.Combine(outputPath, dirName, $"{entityInfo.Name}UpdateDto.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetFilterDto(),
                Path = Path.Combine(outputPath, dirName, $"{entityInfo.Name}FilterDto.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetItemDto(),
                Path = Path.Combine(outputPath, dirName, $"{entityInfo.Name}ItemDto.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetDetailDto(),
                Path = Path.Combine(outputPath, dirName, $"{entityInfo.Name}DetailDto.cs"),
                ModelName = moduleName
            }
        ];
    }
}


public enum DtoType
{
    [Description("Add")]
    Add,
    [Description("Update")]
    Update,
    [Description("Filter")]
    Filter,
    [Description("Item")]
    Item,
    [Description("Detail")]
    Detail
}