using System.Text.RegularExpressions;
using PropertyInfo = Core.Models.PropertyInfo;

namespace CodeGenerator.Generate;
/// <summary>
/// dto generate
/// </summary>
public class DtoCodeGenerate : GenerateBase
{
    public readonly EntityInfo? EntityInfo;
    public string KeyType { get; set; } = "Guid";
    /// <summary>
    /// dto 输出的 程序集名称
    /// </summary>
    public string? AssemblyName { get; set; } = "Share";
    public DtoCodeGenerate(string entityPath, string dtoPath)
    {
        if (File.Exists(entityPath))
        {
            EntityParseHelper entityHelper = new(entityPath);
            EntityInfo = entityHelper.GetEntity();
            AssemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(dtoPath));
            KeyType = EntityInfo.KeyType switch
            {
                EntityKeyType.Int => "Int",
                EntityKeyType.String => "String",
                _ => "Guid"
            };
        }
        else
        {
            _ = new FileNotFoundException();
        }
    }

    /// <summary>
    /// 注释内容替换
    /// </summary>
    /// <param name="comment"></param>
    /// <param name="extendString"></param>
    /// <returns></returns>
    private string FormatComment(string? comment, string extendString = "")
    {
        if (comment == null)
        {
            return "";
        }

        Regex regex = new(@"/// <summary>\r\n/// (?<comment>.*)\r\n/// </summary>");
        Match match = regex.Match(comment);
        if (match.Success)
        {
            string summary = match.Groups["comment"].Value;
            string newComment = summary.Replace("表", "") + extendString;
            comment = comment.Replace(summary, newComment);
        }
        return comment;
    }

    public string? GetShortDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }

        DtoInfo dto = new()
        {
            Name = EntityInfo.Name + "ShortDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "概要"),
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?
                .Where(p => p.Name != "Content"
                    && (p.MaxLength < 2000 || p.MaxLength == null)
                    && !(p.IsList && p.IsNavigation))
                .ToList()
        };
        return dto.ToString(AssemblyName, EntityInfo.Name);
    }

    public string? GetItemDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }

        DtoInfo dto = new()
        {
            Name = EntityInfo.Name + "ItemDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "列表元素"),
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?
                .Where(p => !p.IsList
                    && p.Name != "Content"
                    && (p.MaxLength < 1000 || p.MaxLength == null)
                    && !p.IsNavigation)
                .ToList()
        };
        return dto.ToString(AssemblyName, EntityInfo.Name);
    }

    public string? GetFilterDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }

        List<PropertyInfo>? referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList)
            .Select(s => new PropertyInfo($"{KeyType}?", s.Name + "Id"))
            .ToList();

        string[] filterFields = new string[] { "Id", "CreatedTime", "UpdatedTime", "IsDeleted", "Status" };
        DtoInfo dto = new()
        {
            Name = EntityInfo.Name + "FilterDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "查询筛选"),
            Tag = EntityInfo.Name,
            BaseType = "FilterBase",
        };
        List<PropertyInfo>? properties = EntityInfo.PropertyInfos?
                .Where(p => p.IsRequired
                    || (!p.IsNullable
                        && !p.IsList
                        && !p.IsNavigation
                        && !filterFields.Contains(p.Name))
                    )
                .ToList();
        dto.Properties = properties.Copy() ?? new List<PropertyInfo>();
        // 筛选条件调整为可空
        foreach (PropertyInfo item in dto.Properties)
        {
            item.IsNullable = true;
        }
        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });
        return dto.ToString(AssemblyName, EntityInfo.Name);
    }

    public string? GetAddDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }

        List<PropertyInfo>? referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList)
            .Select(s => new PropertyInfo($"{KeyType}", s.Name + "Id"))
            .ToList();
        DtoInfo dto = new()
        {
            Name = EntityInfo.Name + "AddDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "添加时请求结构"),
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?.Where(p => p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && p.Name != "IsDeleted"
                && p.Name != "Status"
                && !p.IsList
                && !p.IsNavigation)
            .ToList() ?? new List<PropertyInfo>()
        };
        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });
        return dto.ToString(AssemblyName, EntityInfo.Name);
    }

    /// <summary>
    /// 更新dto
    /// 导航属性Name+Id,过滤列表属性
    /// </summary>
    /// <returns></returns>
    public string? GetUpdateDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }
        // 导航属性处理
        List<PropertyInfo>? referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList && !p.IsNullable)
            .Select(s => new PropertyInfo($"{KeyType}", s.Name + "Id"))
            .ToList();
        DtoInfo dto = new()
        {
            Name = EntityInfo.Name + "UpdateDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "更新时请求结构"),
            Tag = EntityInfo.Name,

        };
        // 处理非required的都设置为nullable
        List<PropertyInfo>? properties = EntityInfo.PropertyInfos?.Where(p => p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && p.Name != "IsDeleted"
                && !p.IsList
                && !p.IsNavigation)
            .ToList();

        dto.Properties = properties?.Copy() ?? new List<PropertyInfo>();
        foreach (PropertyInfo item in dto.Properties)
        {
            if (!item.IsRequired)
            {
                item.IsNullable = true;
            }
        }
        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });
        return dto.ToString(AssemblyName, EntityInfo.Name);
    }

    public string GetDtoUsings()
    {
        return @$"global using System.ComponentModel.DataAnnotations;
global using {AssemblyName}.Models;
global using {AssemblyName}.Entities;
global using {EntityInfo!.AssemblyName}.Models;";
    }
    public string GetFilterBase()
    {
        string content = GetTplContent("FilterBase.tpl");
        if (content.NotNull())
        {
            content = content.Replace(TplConst.NAMESPACE, AssemblyName);
        }
        return content;
    }
    public string GetEntityBase()
    {
        string content = GetTplContent("EntityBase.tpl");
        if (content.NotNull())
        {
            content = content.Replace(TplConst.NAMESPACE, AssemblyName)
                .Replace(TplConst.ID_TYPE, Config.IdType)
                .Replace(TplConst.CREATEDTIME_NAME, Config.CreatedTimeName);
        }
        return content;
    }
    public string GetBatchUpdate()
    {
        string content = GetTplContent("BatchUpdate.tpl");
        if (content.NotNull())
        {
            content = content.Replace(TplConst.NAMESPACE, AssemblyName);
        }
        return content;
    }
    public string GetPageList()
    {
        string content = GetTplContent("PageList.tpl");
        if (content.NotNull())
        {
            content = content.Replace(TplConst.NAMESPACE, AssemblyName);
        }
        return content;
    }

    /// <summary>
    /// 生成AutoMapperProfile
    /// </summary>
    /// <param name="entityName"></param>
    protected static void GenerateAutoMapperProfile(string entityName)
    {
        string code =
    @$"            CreateMap<{entityName}AddDto, {entityName}>();
            CreateMap<{entityName}UpdateDto, {entityName}>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => NotNull(srcMember)));;
            CreateMap<{entityName}, {entityName}Dto>();
            CreateMap<{entityName}, {entityName}ItemDto>();
            CreateMap<{entityName}, {entityName}DetailDto>();        
";
        // 先判断是否存在配置文件
        string path = Path.Combine("", "AutoMapper");
        if (!Directory.Exists(path))
        {
            _ = Directory.CreateDirectory(path);
        }
        const string AppendSign = "// {AppendMappers}";
        const string AlreadySign = "// {AlreadyMapedEntity}";
        string mapperFilePath = Path.Combine(path, "AutoGenerateProfile.cs");
        string content;
        if (File.Exists(mapperFilePath))
        {
            // 如果文件存在但当前entity没有生成mapper，则替换该文件
            content = File.ReadAllText(mapperFilePath);
            if (!content.Contains($"// {entityName};"))
            {
                Console.WriteLine("添加Mapper：" + entityName);
                content = content.Replace(AlreadySign, $"// {entityName};\r\n" + AlreadySign);
                content = content.Replace(AppendSign, code + AppendSign);
            }
            else
            {
                Console.WriteLine("已存在:" + entityName);
            }
        }
        else
        {
            // 读取模板文件
            content = GetTplContent("AutoMapper.tpl");
            content = content.Replace(AppendSign, code + AppendSign);
        }
        // 写入文件
        File.WriteAllText(mapperFilePath, content, Encoding.UTF8);
        Console.WriteLine("AutoMapper 配置完成");
    }
}
