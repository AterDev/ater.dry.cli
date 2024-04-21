using System.ComponentModel.DataAnnotations;

namespace Application.Models;


public class UpdateDtoDto
{
    /// <summary>
    /// 文件完整路径
    /// </summary>
    [MaxLength(100)]
    public required string FileName { get; set; }
    [MaxLength(2000)]
    public required string Content { get; set; }

}
