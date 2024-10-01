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
}
