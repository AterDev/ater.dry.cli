using Microsoft.AspNetCore.Http;
using Share;

namespace Application.Implement;

/// <summary>
/// 项目上下文
/// </summary>
public class ProjectContext : IProjectContext
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? SolutionPath { get; set; }
    public string? SharePath { get; set; }
    public string? ApplicationPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }
    public string? EntityFrameworkPath { get; set; }
    public string? ModulesPath { get; set; }

    public ProjectContext(IHttpContextAccessor httpContextAccessor, CommandDbContext context)
    {
        string? id = httpContextAccessor.HttpContext?.Request.Headers["projectId"].ToString();
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (Guid.TryParse(id, out Guid projectId))
            {
                ProjectId = projectId;
                Project = context.Projects.Find(projectId);
                if (Project != null)
                {
                    Const.PROJECT_ID = projectId;
                    SolutionPath = Project.Path;
                    var config = Project.Config;
                    SharePath = Path.Combine(SolutionPath, config.SharePath);
                    ApplicationPath = Path.Combine(SolutionPath, config.ApplicationPath);
                    EntityPath = Path.Combine(SolutionPath, config.EntityPath);
                    ApiPath = Path.Combine(SolutionPath, config.ApiPath);
                    EntityFrameworkPath = Path.Combine(SolutionPath, config.EntityFrameworkPath);
                    ModulesPath = Path.Combine(SolutionPath, PathConst.ModulesPath);
                }
            }
            else
            {
                throw new NullReferenceException("未获取到有效的ProjectId");
            }
        }
    }
}
