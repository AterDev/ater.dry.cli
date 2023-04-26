using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;
/// <summary>
/// 模板内容
/// </summary>
public class TemplateFile : EntityBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [MaxLength(60)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(10_000)]
    public string? Content { get; set; }

}
