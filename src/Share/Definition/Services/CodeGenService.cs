using System.ComponentModel;

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
    public List<GenFileInfo> GenerateDto(EntityInfo entityInfo, string outputPath, bool isCover = false)
    {
        // 生成Dto
        var dtoGen = new DtoCodeGenerate(entityInfo, outputPath);
        var dirName = entityInfo.Name + "Dtos";
        return
        [
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}{Const.AddDto}.cs",
                Content = dtoGen.GetAddDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.AddDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}{Const.UpdateDto}.cs",
                Content = dtoGen.GetUpdateDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.UpdateDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}{Const.FilterDto}.cs",
                Content = dtoGen.GetFilterDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.FilterDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}{Const.ItemDto}.cs",
                Content = dtoGen.GetItemDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.ItemDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo
            {
                IsCover = isCover,
                Name = $"{entityInfo.Name}{Const.DetailDto}.cs",
                Content = dtoGen.GetDetailDto(),
                Path = Path.Combine(outputPath, ModelDirName, dirName, $"{entityInfo.Name}{Const.DetailDto}.cs"),
                ModuleName = entityInfo.ModuleName
            }
        ];
    }


    /// <summary>
    /// 生成manager的文件
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="dtoPath"></param>
    /// <param name="outputPath"></param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public GenFileInfo GenerateManager(EntityInfo entityInfo, string dtoPath, string outputPath, bool isCover = false)
    {
        var managerGen = new ManagerGenerate(entityInfo, dtoPath, outputPath);
        // manager content
        var content = managerGen.GetManagerContent();
        return new GenFileInfo
        {
            IsCover = isCover,
            Name = $"{entityInfo.Name}Manager.cs",
            Content = content,
            Path = Path.Combine(outputPath, Const.Manager, $"{entityInfo.Name}{Const.Manager}.cs"),
            ModuleName = entityInfo.ModuleName
        };

    }

    /// <summary>
    /// Manager服务注入内容
    /// </summary>
    /// <returns></returns>
    public GenFileInfo GetManagerService(EntityInfo entityInfo, string solutionPath)
    {
        string content = ManagerGenerate.GetManagerServiceContent(entityInfo);
        string name = entityInfo.ModuleName.IsEmpty()
            ? "ManagerServiceCollectionExtensions.cs"
            : "ServiceCollectionExtensions.cs";

        return new GenFileInfo
        {
            IsCover = true,
            Name = name,
            Content = content,
            Path = Path.Combine(entityInfo.GetManagerPath(), name),
            ModuleName = entityInfo.ModuleName
        };
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