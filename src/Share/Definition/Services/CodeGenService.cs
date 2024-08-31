using System.ComponentModel;

using CodeGenerator;
using CodeGenerator.Generate;

using Share.Models;

namespace Share.Services;
/// <summary>
/// 代码生成服务
/// </summary>
public class CodeGenService
{
    public const string ModelDirName = "Models";
    public CodeGenService()
    {

    }

    /// <summary>
    /// 生成Dto
    /// </summary>
    /// <param name="entityInfo">实体信息</param>
    /// <param name="outputPath">输出项目目录</param>
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
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.AddDto}.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetUpdateDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.UpdateDto}.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetFilterDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.FilterDto}.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetItemDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.ItemDto}.cs"),
                ModelName = moduleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}Dto.cs",
                Content = dtoGen.GetDetailDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.DetailDto}.cs"),
                ModelName = moduleName
            }
        ];
    }


    public GenFileInfo GenerateManager(EntityInfo entityInfo, string dtoPath, string outputPath, bool isCover = false, string? moduleName = null)
    {

        var managerGen = new ManagerGenerate(entityInfo, dtoPath, outputPath);

        return default;
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