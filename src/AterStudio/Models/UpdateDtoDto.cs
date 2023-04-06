using System.ComponentModel.DataAnnotations;

namespace AterStudio.Models;


public class UpdateDtoDto
{
    [MaxLength(100)]
    public required string FileName { get; set; }
    [MaxLength(2000)]
    public required string Content { get; set; }

}
