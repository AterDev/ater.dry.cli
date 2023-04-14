namespace AterStudio.Models;

public class UpdateConfigOptionsDto
{
    /// <summary>
    /// dto项目目录
    /// </summary>
    public string? DtoPath { get; set; }
    public string? EntityPath { get; set; } 
    public string? StorePath { get; set; } 
    public string? ApiPath { get; set; } 
    public string? IdType { get; set; } 
    public string? CreatedTimeName { get; set; }
    public string? UpdatedTimeName { get; set; }
    public bool? IsSplitController { get; set; }
}
