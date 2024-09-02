﻿using CodeGenerator.Helper;

namespace Application.Manager;

public class ProjectManager(DataAccessContext<Project> dataContext,
    ProjectContext projectContext,
    ILogger<ProjectManager> logger
    ) : ManagerBase<Project>(dataContext, logger)
{
    private readonly ProjectContext _projectContext = projectContext;
    public string GetToolVersion()
    {
        return AssemblyHelper.GetCurrentToolVersion();
    }

    /// <summary>
    /// 获取项目列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<Project>> ListAsync()
    {
        var projects = await Command.ToListAsync();
        for (int i = 0; i < projects.Count; i++)
        {
            var p = projects[i];
            // 移除不存在的项目
            if (!Directory.Exists(p.Path))
            {
                Command.Remove(p);
                projects.Remove(p);
            }
            await SaveChangesAsync();
        }
        return projects;
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(string name, string projectFilePath)
    {
        SolutionType? solutionType = AssemblyHelper.GetSolutionType(projectFilePath);
        var projectName = Path.GetDirectoryName(projectFilePath) ?? name;
        var entity = new Project()
        {
            DisplayName = name,
            Path = projectFilePath,
            Name = projectName,
            SolutionType = solutionType
        };
        return await base.AddAsync(entity) ? entity.Id : null;
    }

    public async Task<Project?> GetDetailAsync(Guid id)
    {
        return await FindAsync(id);
    }

    /// <summary>
    /// 打开项目
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string OpenSolution(string path)
    {
        string res = ProcessHelper.ExecuteCommands($"start {path}");
        return res;
    }

    /// <summary>
    /// 更新解决方案
    /// </summary>
    /// <returns></returns>
    public async Task<string> UpdateSolutionAsync()
    {
        if (_projectContext.Project == null)
        {
            return "未找到有效的项目";
        }
        try
        {
            string path = _projectContext.Project.Path;
            string? version = await AssemblyHelper.GetSolutionVersionAsync(_projectContext.SolutionPath!);
            return version == null ? "未找到项目配置文件，无法进行更新" : "暂不支持该功能";
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message + ex.StackTrace);
            return ex.Message;
        }
    }


    /// <summary>
    /// 更新配置内容
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateConfigAsync(Guid id, ProjectConfig dto)
    {
        var project = await GetCurrentAsync(id);
        project!.Config = dto;
        return await UpdateAsync(project);
    }


    /// <summary>
    /// 添加微服务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool AddServiceProject(string name)
    {
        try
        {
            ProjectCommand.CreateService(_projectContext.SolutionPath, name);
            return true;
        }
        catch (Exception)
        {
            Console.WriteLine("创建服务失败");
            return false;
        }
    }
}
