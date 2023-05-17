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
    public string? ProjectPath { get; set; }
    public string? SharePath { get; set; }
    public string? ApplicationPath { get; set; }
    public string? EntityPath { get; set; }
    public string? ApiPath { get; set; }


    public ProjectContext(IHttpContextAccessor httpContextAccessor, DbContext context)
    {
        var id = httpContextAccessor.HttpContext?.Request.Headers["projectId"].ToString();
        if (id != null)
        {
            ProjectId = Guid.Parse(id);
            Project = context.Projects.FindById(ProjectId);
            Const.PROJECT_ID = ProjectId.Value;
            ProjectPath = GetProjectRootPath(Project.Path);
            var options = ConfigCommand.ReadConfigFile(ProjectPath);
            if (options != null)
            {
                Config.SetConfig(options);
            }

            SharePath = Path.Combine(ProjectPath, Config.DtoPath);
            ApplicationPath = Path.Combine(ProjectPath, Config.StorePath);
            EntityPath = Path.Combine(ProjectPath, Config.EntityPath);
            ApiPath = Path.Combine(ProjectPath, Config.ApiPath);
        }
    }

    public static string GetProjectRootPath(string projectPath)
    {
        return File.Exists(projectPath) ? Path.Combine(projectPath, "..") : projectPath;
    }

}
