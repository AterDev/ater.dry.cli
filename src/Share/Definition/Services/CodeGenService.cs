using System.ComponentModel;
using CodeGenerator.Generate;
using CodeGenerator.Models;

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
    /// <param name="outputPath">输出项目目录</param>
    /// <param name="isCover">是否覆盖</param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateDto(EntityInfo entityInfo, string outputPath, bool isCover = false)
    {
        // 生成Dto
        var dtoGen = new DtoCodeGenerate(entityInfo);
        var dirName = entityInfo.Name + "Dtos";
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, dtoGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(Const.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            Path = Path.Combine(outputPath, Const.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };

        return
        [
            globalFile,
            new GenFileInfo($"{entityInfo.Name}{Const.AddDto}.cs", dtoGen.GetAddDto())
            {
                IsCover = isCover,
                Path = Path.Combine(outputPath, Const.ModelsDir, dirName, $"{entityInfo.Name}{Const.AddDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo( $"{entityInfo.Name}{Const.UpdateDto}.cs", dtoGen.GetUpdateDto())
            {
                IsCover = isCover,
                Path = Path.Combine(outputPath, Const.ModelsDir, dirName, $"{entityInfo.Name}{Const.UpdateDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo( $"{entityInfo.Name}{Const.FilterDto}.cs", dtoGen.GetFilterDto())
            {
                IsCover = isCover,
                Path = Path.Combine(outputPath, Const.ModelsDir, dirName, $"{entityInfo.Name}{Const.FilterDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo($"{entityInfo.Name}{Const.ItemDto}.cs", dtoGen.GetItemDto())
            {
                IsCover = isCover,
                Path = Path.Combine(outputPath, Const.ModelsDir, dirName, $"{entityInfo.Name}{Const.ItemDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo($"{entityInfo.Name}{Const.DetailDto}.cs", dtoGen.GetDetailDto())
            {
                IsCover = isCover,
                Path = Path.Combine(outputPath, Const.ModelsDir, dirName, $"{entityInfo.Name}{Const.DetailDto}.cs"),
                ModuleName = entityInfo.ModuleName
            }
        ];
    }

    /// <summary>
    /// 生成manager的文件
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="outputPath"></param>
    /// <param name="tplContent">模板内容</param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateManager(EntityInfo entityInfo, string outputPath, string tplContent, bool isCover = false)
    {
        var managerGen = new ManagerGenerate(entityInfo);
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, managerGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(Const.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            Path = Path.Combine(outputPath, Const.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };

        var content = managerGen.GetManagerContent(tplContent, entityInfo.GetManagerNamespace());
        var managerFile = new GenFileInfo($"{entityInfo.Name}{Const.Manager}.cs", content)
        {
            IsCover = isCover,
            Path = Path.Combine(outputPath, Const.Manager, $"{entityInfo.Name}{Const.Manager}.cs"),
            ModuleName = entityInfo.ModuleName
        };

        var managerService = GetManagerService(entityInfo);
        return [globalFile, managerFile, managerService];
    }

    /// <summary>
    /// Manager服务注入内容
    /// </summary>
    /// <returns></returns>
    public GenFileInfo GetManagerService(EntityInfo entityInfo)
    {
        string content = ManagerGenerate.GetManagerServiceContent(entityInfo);
        string name = entityInfo.ModuleName.IsEmpty()
            ? Const.ServiceExtensionsFile
            : Const.ManagerServiceExtensionsFile;

        return new GenFileInfo(name, content)
        {
            IsCover = true,
            Path = Path.Combine(entityInfo.GetManagerPath(), name),
            ModuleName = entityInfo.ModuleName
        };
    }

    /// <summary>
    /// RestAPI生成
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="outputPath"></param>
    /// <param name="tplContent"></param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateController(EntityInfo entityInfo, string outputPath, string tplContent, bool isCover = false)
    {
        var apiGen = new RestApiGenerate(entityInfo);
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, apiGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(Const.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            Path = Path.Combine(outputPath, Const.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };
        var content = apiGen.GetRestApiContent(tplContent);
        var controllerFile = new GenFileInfo($"{entityInfo.Name}{Const.Manager}.cs", content)
        {
            IsCover = isCover,
            Path = Path.Combine(outputPath, Const.ManagersDir, $"{entityInfo.Name}{Const.Manager}.cs"),
            ModuleName = entityInfo.ModuleName
        };
        return [globalFile, controllerFile];
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