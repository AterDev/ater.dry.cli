using Microsoft.AspNetCore.Http;

namespace Application.Implement;

/// <summary>
/// 项目上下文
/// </summary>
public class ProjectContext
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? SolutionPath { get; set; }
    public string? SharePath { get; set; }
    public string? ApplicationPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }
    public string? Version { get; set; }
    public string? EntityFrameworkPath { get; set; }

    public ProjectContext(IHttpContextAccessor httpContextAccessor, CommandDbContext context)
    {
        var id = httpContextAccessor.HttpContext?.Request.Headers["projectId"].ToString();
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (Guid.TryParse(id, out Guid projectId))
            {
                ProjectId = projectId;
                Project = context.Projects.Find(projectId);
                if (Project != null)
                {
                    Const.PROJECT_ID = projectId;
                    SolutionPath = GetProjectRootPath(Project.Path);
                    var options = ConfigCommand.ReadConfigFile(SolutionPath);
                    if (options != null)
                    {
                        Config.SetConfig(options);
                        Config.SolutionPath = SolutionPath;
                    }

                    SharePath = Path.Combine(SolutionPath, Config.SharePath);
                    ApplicationPath = Path.Combine(SolutionPath, Config.ApplicationPath);
                    EntityPath = Path.Combine(SolutionPath, Config.EntityPath);
                    ApiPath = Path.Combine(SolutionPath, Config.ApiPath);
                    EntityFrameworkPath = Path.Combine(SolutionPath, Config.EntityFrameworkPath);
                    Version = AssemblyHelper.GetSolutionVersionAsync(SolutionPath).Result;
                }
            }
            else
            {
                throw new NullReferenceException("未获取到有效的ProjectId");
            }
        }
    }

    public static string GetProjectRootPath(string projectPath)
    {
        return File.Exists(projectPath) ? Path.GetDirectoryName(projectPath)! : projectPath;
    }

}
