using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datastore.Models;

/// <summary>
/// 属性变化
/// </summary>
public class PropertyChange
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public required string Name { get; set; }
    public required ChangeType Type { get; set; }
}

public enum ChangeType
{
    Add,
    Update,
    Delete,
}
