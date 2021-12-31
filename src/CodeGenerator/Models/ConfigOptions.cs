namespace CodeGenerator.Models;

public class ConfigOptions
{
    /// <summary>
    /// 项目目录，必须
    /// </summary>
    public string ProjectPath { get; set; } = string.Empty;
    /// <summary>
    /// dto项目目录
    /// </summary>
    public string DtoPath { get; set; } = string.Empty;

    public string? EntityNamespace { get; set; }
    public string? ServiceNamespace { get; set; }
    public string? ApiNamespace { get; set; }
    public string? ShareNamespace { get; set; }
    public string? DbContextNamespace { get; set; }

    public string? BasePath { get; set; }
    public string? EntityPath { get; set; }
    public string? DbContextPath { get; set; }
    public string? ServicePath { get; set; }
    public string? SharePath { get; set; }
    public string? ApiPath { get; set; }

    public string? ClientPath { get; set; }
    
}
