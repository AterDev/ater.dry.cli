using Command.Share.Commands;

using Core;
using Core.Entities;
using Core.Infrastructure;

using Datastore;

namespace AterStudio;

/// <summary>
/// 项目上下文
/// </summary>
public class ProjectContext
{
    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? SolutionPath { get; set; }
    public string? SharePath { get; set; }
    public string? ApplicationPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }


    public ProjectContext(IHttpContextAccessor httpContextAccessor, DbContext context)
    {
        var id = httpContextAccessor.HttpContext?.Request.Headers["projectId"].ToString();
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (Guid.TryParse(id, out Guid ProjectId))
            {
                Project = context.Projects.FindById(ProjectId);
                if (Project != null)
                {
                    Const.PROJECT_ID = ProjectId;
                    SolutionPath = GetProjectRootPath(Project.Path);
                    var options = ConfigCommand.ReadConfigFile(SolutionPath);
                    if (options != null)
                    {
                        Config.SetConfig(options);
                        Config.SolutionPath = SolutionPath;
                    }

                    SharePath = Path.Combine(SolutionPath, Config.DtoPath);
                    ApplicationPath = Path.Combine(SolutionPath, Config.StorePath);
                    EntityPath = Path.Combine(SolutionPath, Config.EntityPath);
                    ApiPath = Path.Combine(SolutionPath, Config.ApiPath);
                }
            }
        }
    }

    public static string GetProjectRootPath(string projectPath)
    {
        return File.Exists(projectPath) ? Path.GetDirectoryName(projectPath)! : projectPath;
    }

}
