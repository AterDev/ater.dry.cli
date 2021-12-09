namespace Droplet.CommandLine.Commands
{
    public class DtoGenerate : GenerateBase
    {
        public string EntityPath { get; set; }
        public string DtoPath { get; set; }

        public DtoGenerate(string entityPath, string dtoPath)
        {
            EntityPath = entityPath;
            DtoPath = dtoPath;
        }
        /// <summary>
        /// 生成dtos
        /// <param name="force">覆盖</param>
        /// </summary>
        public void GenerateDtos(bool force = false)
        {
            if (!File.Exists(EntityPath))
            {
                Console.WriteLine(EntityPath + " not found!");
                return;
            }
            Console.WriteLine("开始解析实体");
            var typeHelper = new ClassParseHelper(EntityPath);
            var properties = typeHelper.PropertyInfos;
            string className = typeHelper.Name;
            string comment = typeHelper.Comment;

            // 创建相关dto文件
            var referenceProps = properties.Where(p => p.IsReference)
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
                    && !p.IsReference)
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
                    && !p.IsReference).ToList()
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
                Properties = properties.Where(p => !p.IsList && p.Name != "UpdatedTime" && !p.IsReference).ToList()
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
            addDto.Save(DtoPath, force);
            updateDto.Save(DtoPath, force);
            ListDto.Save(DtoPath, force);
            ItemDto.Save(DtoPath, force);
            DetailDto.Save(DtoPath, force);
            FilterDto.Save(DtoPath, force);
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
            string code =
@$"            CreateMap<{entityName}AddDto, {entityName}>();
            CreateMap<{entityName}UpdateDto, {entityName}>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => NotNull(srcMember)));;
            CreateMap<{entityName}, {entityName}Dto>();
            CreateMap<{entityName}, {entityName}ItemDto>();
            CreateMap<{entityName}, {entityName}DetailDto>();        
";
            // 先判断是否存在配置文件
            var path = Path.Combine(DtoPath, "AutoMapper");
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

        public class DtoInfo
        {
            public string Name { get; set; }
            public string BaseType { get; set; }
            public List<PropertyInfo> Properties { get; set; }
            public string Tag { get; set; }
            public string NamespaceName { get; set; }
            public string Comment { get; set; }

            public override string ToString()
            {
                var propStrings = string.Join(string.Empty, Properties.Select(p => p.ToCsharpLine()).ToArray());
                var baseType = string.IsNullOrEmpty(BaseType) ? "" : " : " + BaseType;
                var tpl = $@"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using {Config.SHARE_NAMESPACE}.Models;
using {NamespaceName};
namespace Share.Models
{{
    {Comment}
    public class {Name}{baseType}
    {{
{propStrings}    
    }}
}}";
                return tpl;
            }
            public void Save(string dir, bool cover)
            {
                var path = Path.Combine(dir, Tag);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = Path.Combine(path, Name + ".cs");
                // 不覆盖
                if (!cover && File.Exists(fileName))
                {
                    Console.WriteLine("skip dto file:" + fileName);
                    return;
                }
                File.WriteAllText(fileName, ToString());
                Console.WriteLine("Created dto file:" + fileName);
            }
        }
    }
}
