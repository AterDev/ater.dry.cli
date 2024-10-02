namespace Share;
/// <summary>
/// 项目上下文
/// </summary>
public interface IProjectContext
{
    Guid ProjectId { get; set; }
    Project? Project { get; set; }
    string? SolutionPath { get; set; }
    string? SharePath { get; set; }
    string? ApplicationPath { get; set; }
    string? EntityPath { get; set; }
    string? ApiPath { get; set; }
    string? EntityFrameworkPath { get; set; }
    string? ModulesPath { get; set; }

    string GetApiPath(string? moduleName = null);
    string GetApplicationPath(string? moduleName = null);
    string GetControllerPath(string? moduleName = null);
    string GetDtoPath(string entityName, string? moduleName = null);
    string GetManagerPath(string? moduleName = null);
    string GetSharePath(string? moduleName = null);
}
