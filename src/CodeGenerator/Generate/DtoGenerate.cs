using PropertyInfo = CodeGenerator.Models.PropertyInfo;

namespace CodeGenerator.Generate;
/// <summary>
/// dto generate
/// </summary>
public class DtoGenerate : GenerateBase
{
    public string? EntityContent { get; set; }
    public string? EntityPath { get; set; }
    public DtoGenerate(string entityPath)
    {
        if (File.Exists(entityPath))
        {
            EntityPath = entityPath;
            EntityContent = File.ReadAllText(EntityPath);
        }
        else
        {
            //TODO: 
            _ = new FileNotFoundException();
        }
    }

    /// <summary>
    /// 生成dtos
    /// <param name="force">覆盖</param>
    /// </summary>
    public void GenerateDtos(bool force = false)
    {
        Console.WriteLine("开始解析实体");
        var typeHelper = new EntityParseHelper(EntityPath);
        var properties = typeHelper.PropertyInfos;
        var className = typeHelper.Name;
        var comment = typeHelper.Comment;

        // 创建相关dto文件
        var referenceProps = properties.Where(p => p.IsNavigation)
                .Select(s => new PropertyInfo("Guid?", s.Name + "Id"))
                .ToList();
        var addDto = new DtoInfo
        {
            Name = className + "AddDto",
            NamespaceName = typeHelper.NamespaceName,
            Comment = comment,
            Tag = className,
            Properties = properties.Where(p => p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && !p.IsList
                && !p.IsNavigation)
            .ToList()
        };
        foreach (var item in referenceProps)
        {
            if (!addDto.Properties.Any(p => p.Name == item.Name))
            {
                addDto.Properties.Add(item);
            }
        }
        var updateDto = new DtoInfo
        {
            Name = className + "UpdateDto",
            NamespaceName = typeHelper.NamespaceName,
            Comment = comment,
            Tag = className,
            Properties = properties.Where(p => p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && !p.IsList
                && !p.IsNavigation).ToList()
        };
        // 列表项dto
        var ListDto = new DtoInfo
        {
            Name = className + "Dto",
            NamespaceName = typeHelper.NamespaceName,
            Comment = comment,
            Tag = className,
            Properties = properties.Where(p => !p.IsList).ToList()
        };
        var ItemDto = new DtoInfo
        {
            Name = className + "ItemDto",
            NamespaceName = typeHelper.NamespaceName,
            Comment = comment,
            Tag = className,
            Properties = properties.Where(p => !p.IsList && p.Name != "UpdatedTime" && !p.IsNavigation).ToList()
        };
        var DetailDto = new DtoInfo
        {
            Name = className + "DetailDto",
            NamespaceName = typeHelper.NamespaceName,
            Comment = comment,
            Tag = className,
            Properties = properties
        };
        var FilterDto = new DtoInfo
        {
            Name = className + "Filter",
            NamespaceName = typeHelper.NamespaceName,
            Comment = comment,
            Tag = className,
            BaseType = "FilterBase",
            Properties = referenceProps
        };
        // TODO:可能存在自身到自身的转换
        //addDto.Save(DtoPath, force);
        //updateDto.Save(DtoPath, force);
        //ListDto.Save(DtoPath, force);
        //ItemDto.Save(DtoPath, force);
        //DetailDto.Save(DtoPath, force);
        //FilterDto.Save(DtoPath, force);
        Console.WriteLine("生成dto模型完成");

        // 添加autoMapper配置
        GenerateAutoMapperProfile(className);
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
