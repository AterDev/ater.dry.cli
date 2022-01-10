using PropertyInfo = CodeGenerator.Models.PropertyInfo;

namespace CodeGenerator.Generate;
/// <summary>
/// dto generate
/// </summary>
public class DtoGenerate : GenerateBase
{
    public EntityInfo? EntityInfo { get; set; }
    public DtoGenerate(string entityPath)
    {
        if (File.Exists(entityPath))
        {
            var entityHelper = new EntityParseHelper(entityPath);
            EntityInfo = entityHelper.GetEntity();
        }
        else
        {
            _ = new FileNotFoundException();
        }
    }

    /// <summary>
    /// 生成dtos
    /// <param name="force">覆盖</param>
    /// </summary>
    public void GenerateDtos(bool force = false)
    {
        //Console.WriteLine("开始解析实体");

        // 列表项dto
        var ListDto = new DtoInfo
        {
            Name = EntityInfo.Name + "Dto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos.Where(p => !p.IsList).ToList()
        };
        var ItemDto = new DtoInfo
        {
            Name = EntityInfo.Name + "ItemDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos.Where(p => !p.IsList && p.Name != "UpdatedTime" && !p.IsNavigation).ToList()
        };
        var DetailDto = new DtoInfo
        {
            Name = EntityInfo.Name + "DetailDto",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos
        };
        var FilterDto = new DtoInfo
        {
            Name = EntityInfo.Name + "Filter",
            NamespaceName = EntityInfo.NamespaceName,
            Comment = EntityInfo.Comment,
            Tag = EntityInfo.Name,
            BaseType = "FilterBase",
            //Properties = referenceProps
        };
        // 添加autoMapper配置
        //GenerateAutoMapperProfile(EntityInfo.Name);
    }

    public string? GetAddDto()
    {
        if (EntityInfo == null) return default;
        var referenceProps = EntityInfo.PropertyInfos?.Where(p => p.IsNavigation)
               .Select(s => new PropertyInfo("Guid?", s.Name + "Id"))
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
        foreach (var item in referenceProps)
        {
            if (!dto.Properties.Any(p => p.Name == item.Name))
            {
                dto.Properties.Add(item);
            }
        }
        return dto.ToString();
    }

    public string? GetUpdateDto()
    {
        if (EntityInfo == null) return default;
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
