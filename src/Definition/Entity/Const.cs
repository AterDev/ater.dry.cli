namespace Entity;
public class Const
{
    public static Guid PROJECT_ID;

    public const string ConfigFileName = ".dry-config.json";
    public const string StudioFileName = "AterStudio.dll";

    // assembly name
    public const string ApplicationName = "Application";
    public const string ShareName = "Share";
    public const string EntityName = "Entity";
    public const string APIName = "Http.API";
    public const string EntityFrameworkName = "EntityFramework";

    // dir names
    public const string DefinitionDir = "Definition";
    public const string ModulesDir = "Modules";
    public const string ModelsDir = "Models";
    public const string ManagersDir = "Managers";
    public const string ControllersDir = "Controllers";
    public const string SrcDir = "src";
    public const string MicroserviceDir = "Microservice";

    public const string StudioDir = "DryStudio";


    // names
    public const string Manager = "Manager";
    public const string Controller = "Controller";
    public const string DetailDto = "DetailDto";
    public const string ItemDto = "ItemDto";
    public const string FilterDto = "FilterDto";
    public const string AddDto = "AddDto";
    public const string UpdateDto = "UpdateDto";

    // props & keys
    public const string FilterBase = "FilterBase";
    public const string EntityBase = "EntityBase";

    public const string Id = "Id";
    public const string Guid = "Guid";
    public const string CreatedTime = "CreatedTime";
    public const string UpdatedTime = "UpdatedTime";
    public const string IsDeleted = "IsDeleted";


    public const string Version = "9.0.0";
    public const string NetVersion = "net9.0";
    public const string PackageId = "ater.dry";

    // files 
    public const string TemplateZip = "template.zip";
    public const string StudioZip = "studio.zip";

    public const string SolutionExtension = ".sln";
    public const string SolutionXMLExtension = ".slnx";
    public const string CSharpProjectExtension = ".csproj";
    public const string NodeProjectFile = "package.json";
    public const string CoreLibName = "Ater.Web.Core";
    public const string AbstractionLibName = "Ater.Web.Abstraction";
    public const string ExtensionLibName = "Ater.Web.Extension";

    public const string GlobalUsingsFile = "GlobalUsings.cs";
    public const string ServiceExtensionsFile = "ServiceCollectionExtensions.cs";
    public const string ManagerServiceExtensionsFile = "ManagerServiceCollectionExtensions.cs";
}

/// <summary>
///在默认路径
/// </summary>
public class PathConst
{
    public readonly static string APIPath = Path.Combine(Const.SrcDir, Const.APIName);
    public readonly static string ApplicationPath = Path.Combine(Const.SrcDir, Const.ApplicationName);
    public readonly static string DefinitionPath = Path.Combine(Const.SrcDir, Const.DefinitionDir);
    public readonly static string SharePath = Path.Combine(Const.SrcDir, Const.DefinitionDir, Const.ShareName);
    public readonly static string EntityPath = Path.Combine(Const.SrcDir, Const.DefinitionDir, Const.EntityName);
    public readonly static string EntityFrameworkPath = Path.Combine(Const.SrcDir, Const.DefinitionDir, Const.EntityFrameworkName);
    public readonly static string ModulesPath = Path.Combine(Const.SrcDir, Const.ModulesDir);
    public readonly static string MicroservicePath = Path.Combine(Const.SrcDir, Const.MicroserviceDir);
}
