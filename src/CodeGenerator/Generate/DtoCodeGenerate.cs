using System.Text.RegularExpressions;
using Datastore;
using Datastore.Models;
using Microsoft.EntityFrameworkCore;
using PropertyInfo = Core.Models.PropertyInfo;

namespace CodeGenerator.Generate;
/// <summary>
/// dto generate
/// </summary>
public class DtoCodeGenerate : GenerateBase
{
    public EntityInfo EntityInfo { get; init; }
    public string KeyType { get; set; } = "Guid";
    /// <summary>
    /// dto 输出的 程序集名称
    /// </summary>
    public string? AssemblyName { get; set; } = "Share";
    public string DtoPath { get; init; }
    public ContextBase _context { get; init; }

    public List<PropertyChange> PropertyChanges = new List<PropertyChange>();

    public DtoCodeGenerate(string entityPath, string dtoPath)
    {
        _context = new ContextBase();
        DtoPath = dtoPath;
        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException();
        }

        var entityHelper = new EntityParseHelper(entityPath);
        EntityInfo = entityHelper.GetEntity();
        AssemblyName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(DtoPath));
        KeyType = EntityInfo.KeyType switch
        {
            EntityKeyType.Int => "Int",
            EntityKeyType.String => "String",
            _ => "Guid"
        };
        GetChangedPropertiesAsync().Wait();
    }

    /// <summary>
    /// 获取变更的实体属性内容
    /// </summary>
    public async Task GetChangedPropertiesAsync()
    {
        var currentEntity = await _context.EntityInfos
            .Where(e => e.Name == EntityInfo.Name
                && e.NamespaceName == EntityInfo.NamespaceName
                && e.ProjectId == Const.PROJECT_ID)
            .Include(e => e.PropertyInfos)
            .FirstOrDefaultAsync();

        if (currentEntity != null)
        {
            // 新的属性名称
            var propNames = EntityInfo.PropertyInfos.Select(p => p.Name).ToList();

            // 变动的属性
            var updateProps = EntityInfo.PropertyInfos
                .Except(currentEntity.PropertyInfos, new PropertyEquality())
                .ToList();

            // 待删除的属性
            var removeProps = currentEntity.PropertyInfos
                .Where(p => !propNames.Contains(p.Name))
                .ToList();

            updateProps.ForEach(p =>
            {
                var prop = new PropertyChange
                {
                    Name = p.Name,
                    Type = ChangeType.Update
                };
                PropertyChanges.Add(prop);
            });

            removeProps.ForEach(p =>
            {
                var prop = new PropertyChange
                {
                    Name = p.Name,
                    Type = ChangeType.Delete
                };
                PropertyChanges.Add(prop);
            });
            _context.RemoveRange(currentEntity.PropertyInfos);
            _context.Remove(currentEntity);
        }
        await _context.AddAsync(EntityInfo);
        await _context.SaveChangesAsync();

        PropertyChanges.ForEach(p =>
        {
            Console.WriteLine(p.Type.ToString() + " : " + p.Name);
        });
    }

    /// <summary>
    /// 合并更新后的属性
    /// </summary>
    /// <param name="dtoPath">dto文件的完整路径</param>
    /// <returns></returns>
    public List<PropertyInfo> MergeProperties(string dtoPath)
    {
        if (!File.Exists(dtoPath))
        {
            Console.WriteLine("not found:" + dtoPath);
            return EntityInfo.PropertyInfos;
        }

        var entityHelper = new EntityParseHelper(dtoPath);
        var entityInfo = entityHelper.GetEntity();
        var props = entityInfo.PropertyInfos;

        //Console.WriteLine("before change:" + string.Join(",", props.Select(p => p.Name).ToArray()));

        // 1 移除删除的内容
        var deletePropNames = PropertyChanges.Where(c => c.Type == ChangeType.Delete)
            .Select(c => c.Name).ToList();
        props = props.Where(p => !deletePropNames.Contains(p.Name)).ToList();

        Console.WriteLine("remove props:" + string.Join(",", deletePropNames));

        // 2 要更新的属性
        var updatePropNames = PropertyChanges.Where(c => c.Type != ChangeType.Delete)
            .Select(c => c.Name).ToList();

        Console.WriteLine("update props:" + string.Join(",", updatePropNames));
        var updateProps = EntityInfo.PropertyInfos.Where(p => updatePropNames.Contains(p.Name)).ToList();
        updateProps.ForEach(p =>
        {
            var index = props.FindIndex(item => item.Name == p.Name);
            if (index > -1)
            {
                props[index] = p;
            }
            else
            {
                props.Add(p);
            }
        });

        //Console.WriteLine("after change:" + string.Join(",", props.Select(p => p.Name).ToArray()));
        return props;
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

        var dtoFileName = EntityInfo.Name + Const.ShortDto + ".cs";
        var dtoFilePath = Path.Combine(DtoPath, "Models", EntityInfo.Name + "Dtos", dtoFileName);
        var props = MergeProperties(dtoFilePath);

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.ShortDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "概要"),
            Tag = EntityInfo.Name,
            Properties = props?
                .Where(p => p.Name != "Content"
                    && p.Name != "IsDeleted"
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

        var dtoFileName = EntityInfo.Name + Const.ItemDto + ".cs";
        var dtoFilePath = Path.Combine(DtoPath, "Models", EntityInfo.Name + "Dtos", dtoFileName);
        var props = MergeProperties(dtoFilePath);

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.ItemDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "列表元素"),
            Tag = EntityInfo.Name,
            Properties = props?
                .Where(p => !p.IsList
                    && p.Name != "IsDeleted"
                    && (p.MaxLength <= 1000 || p.MaxLength == null)
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
            .Select(s => new PropertyInfo($"{KeyType}?", s.Name + "Id")
            {
                ProjectId = Const.PROJECT_ID
            })
            .ToList();

        string[] filterFields = new string[] { "Id", "CreatedTime", "UpdatedTime", "IsDeleted", "Status", "PageSize", "PageIndex" };

        var dtoFileName = EntityInfo.Name + Const.FilterDto + ".cs";
        var dtoFilePath = Path.Combine(DtoPath, "Models", EntityInfo.Name + "Dtos", dtoFileName);
        var props = MergeProperties(dtoFilePath);

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.FilterDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "查询筛选"),
            Tag = EntityInfo.Name,
            BaseType = "FilterBase",
        };

        List<PropertyInfo>? properties = props?
                .Where(p => (p.IsRequired && !p.IsNavigation)
                    || (!p.IsList
                        && !p.IsNavigation
                        && !filterFields.Contains(p.Name)
                     || p.IsEnum)
                    )
                .Where(p => p.MaxLength is not (not null and >= 1000))
                .ToList();
        dto.Properties = properties.Copy() ?? new List<PropertyInfo>();
        // 筛选条件调整为可空
        foreach (var item in dto.Properties)
        {
            item.IsNullable = true;
            item.IsRequired = false;
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
            .Select(s => new PropertyInfo($"{KeyType}", s.Name + "Id")
            {
                ProjectId = Const.PROJECT_ID
            })
            .ToList();

        var dtoFileName = EntityInfo.Name + Const.AddDto + ".cs";
        var dtoFilePath = Path.Combine(DtoPath, "Models", EntityInfo.Name + "Dtos", dtoFileName);
        var props = MergeProperties(dtoFilePath);

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.AddDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "添加时请求结构"),
            Tag = EntityInfo.Name,
            Properties = props?.Where(p => p.Name != "Id"
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
    /// 导航属性 Name+Id,过滤列表属性
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
            .Select(s => new PropertyInfo($"{KeyType}", s.Name + "Id")
            {
                ProjectId = Const.PROJECT_ID,
                IsRequired = s.IsRequired
            })
            .ToList();

        var dtoFileName = EntityInfo.Name + Const.UpdateDto + ".cs";
        var dtoFilePath = Path.Combine(DtoPath, "Models", EntityInfo.Name + "Dtos", dtoFileName);
        var props = MergeProperties(dtoFilePath);

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.UpdateDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "更新时请求结构"),
            Tag = EntityInfo.Name,

        };
        // 处理非required的都设置为nullable
        List<PropertyInfo>? properties = props?.Where(p => p.Name != "Id"
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
