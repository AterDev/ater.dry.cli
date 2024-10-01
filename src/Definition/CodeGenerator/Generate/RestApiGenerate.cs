namespace CodeGenerator.Generate;

/// <summary>
/// 生成Rest API控制器
/// </summary>
public class RestApiGenerate(EntityInfo entityInfo)
{
    public string? EntityNamespace { get; set; } = entityInfo.NamespaceName;
    /// <summary>
    /// DataStore 项目的命名空间
    /// </summary>
    public string? ShareNamespace { get; set; } = entityInfo.GetShareNamespace();
    public string? ApplicationNamespace { get; set; } = entityInfo.GetManagerNamespace();
    public string? ApiNamespace { get; set; } = entityInfo.GetControllerPath();
    public EntityInfo EntityInfo { get; init; } = entityInfo;

    public List<string> GetGlobalUsings()
    {
        return
        [
            "global using Microsoft.Extensions.DependencyInjection;",
            "global using Microsoft.AspNetCore.Mvc;",
            "global using Microsoft.AspNetCore.Authorization;",
            "global using System.Text.Json.Serialization;",
            "global using Microsoft.EntityFrameworkCore;",
            $"global using {Const.CoreLibName}.Models;",
            $"global using {Const.CoreLibName}.Utils;",
            $"global using {Const.AbstractionLibName};",
            $"global using {Const.ExtensionLibName}.Services;",
            $"global using {EntityInfo.NamespaceName};",
            $"global using {ApplicationNamespace}.{Const.ManagersDir};",
            $"global using {ApplicationNamespace}.Implement;",
        ];
    }

    /// <summary>
    /// 生成控制器
    /// </summary>
    public string GetRestApiContent(string tplContent)
    {
        var genContext = new GenContext();
        var model = new ControllerViewModel
        {
            Namespace = EntityInfo.GetAPINamespace(),
            EntityName = EntityInfo.Name,
            ShareNamespace = ShareNamespace,
        };
        return genContext.GenCode(tplContent, model);
    }
}
