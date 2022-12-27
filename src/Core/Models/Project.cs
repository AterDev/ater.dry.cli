namespace Core.Models;
/// <summary>
/// 项目
/// </summary>
public class Project : EntityBase
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string Path { get; set; }
    public string EntityPath { get; set; } = "./Core";
    public string SharePath { get; set; } = "./Share";
    public string ApplicationPath { get; set; } = "./Application";
    public string HttpPath { get; set; } = "./Http.API";
    public string EntityFrameworkPath { get; set; } = "./Database/EntityFramework";

    /// <summary>
    /// swagger地址
    /// </summary>
    public string? SwaggerPath { get; set; }
    /// <summary>
    /// 前端路径
    /// </summary>
    public string? WebAppPath { get; set; }
}
