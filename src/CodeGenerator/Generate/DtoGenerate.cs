using static CodeGenerator.Models.EntityInfo;
using PropertyInfo = CodeGenerator.Models.PropertyInfo;

namespace CodeGenerator.Generate;
/// <summary>
/// dto generate
/// </summary>
public class DtoGenerate : GenerateBase
{
    public EntityInfo? EntityInfo { get; set; }
    public string KeyType { get; set; } = "Guid";
    public DtoGenerate(string entityPath)
    {
        if (File.Exists(entityPath))
        {
            var entityHelper = new EntityParseHelper(entityPath);
            EntityInfo = entityHelper.GetEntity();
            KeyType = (EntityInfo.KeyType) switch
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

    public string? GetShortDto()
    {
        var dto = new DtoInfo
        {
            Name = EntityInfo.Name + "DetailDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos
        };
        return dto.ToString();
    }

    public string? GetItemDto()
    {
        var dto = new DtoInfo
        {
            Name = EntityInfo.Name + "ItemDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos.Where(p => !p.IsList && p.Name != "UpdatedTime" && !p.IsNavigation).ToList()
        };
        return dto.ToString();
    }

    public string? GetFilterDto()
    {
        if (EntityInfo == null) return default;
        var referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList)
            .Select(s => new PropertyInfo("Guid?", s.Name + "Id"))
            .ToList();
        var dto = new DtoInfo
        {
            Name = EntityInfo.Name + "Filter",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            BaseType = "FilterBase",
            Properties = EntityInfo.PropertyInfos?
                .Where(p => p.IsRequired
                    || (!p.IsNullable && !p.IsList))
                .ToList()
        };
        return dto.ToString();
    }

    public string? GetAddDto()
    {
        if (EntityInfo == null) return default;
        var referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList)
            .Select(s => new PropertyInfo("Guid", s.Name + "Id"))
            .ToList();
        var dto = new DtoInfo
        {
            Name = EntityInfo.Name + "AddDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?.Where(p => p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && !p.IsList
                && !p.IsNavigation)
            .ToList()
        };
        referenceProps?.ForEach(item =>
        {
            dto.Properties?.Add(item);
        });
        return dto.ToString();
    }

    /// <summary>
    /// 更新dto
    /// 导航属性Name+Id,过滤列表属性
    /// </summary>
    /// <returns></returns>
    public string? GetUpdateDto()
    {
        if (EntityInfo == null) return default;
        // 导航属性处理
        var referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList)
            .Select(s => new PropertyInfo("Guid?", s.Name + "Id"))
            .ToList();
        var dto = new DtoInfo
        {
            Name = EntityInfo.Name + "UpdateDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?.Where(p => p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && !p.IsList
                && !p.IsNavigation).ToList()
        };
        referenceProps?.ForEach(item =>
        {
            dto.Properties?.Add(item);
        });
        return dto.ToString();
    }
    /// <summary>
    /// 生成AutoMapperProfile
    /// </summary>
    /// <param name="entityName"></param>
    protected void GenerateAutoMapperProfile(string entityName)
    {
        var code =
@$"            CreateMap<{entityName}AddDto, {entityName}>();
            CreateMap<{entityName}UpdateDto, {entityName}>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => NotNull(srcMember)));;
            CreateMap<{entityName}, {entityName}Dto>();
            CreateMap<{entityName}, {entityName}ItemDto>();
            CreateMap<{entityName}, {entityName}DetailDto>();        
";
        // 先判断是否存在配置文件
        var path = Path.Combine("", "AutoMapper");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        const string AppendSign = "// {AppendMappers}";
        const string AlreadySign = "// {AlreadyMapedEntity}";
        var mapperFilePath = Path.Combine(path, "AutoGenerateProfile.cs");
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
