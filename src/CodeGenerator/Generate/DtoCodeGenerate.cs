using System.Text.RegularExpressions;
using Core.Entities;
using Datastore;
using Datastore.Models;
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
    public List<PropertyChange> PropertyChanges = [];
    public readonly DbContext dbContext;
    public DtoCodeGenerate(string entityPath, string dtoPath, DbContext dbContext)
    {
        DtoPath = dtoPath;
        this.dbContext = dbContext;
        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException();
        }

        var entityHelper = new EntityParseHelper(entityPath);
        EntityInfo = entityHelper.GetEntity();
        KeyType = EntityInfo.KeyType switch
        {
            EntityKeyType.Int => "Int",
            EntityKeyType.String => "String",
            _ => "Guid"
        };
    }

    /// <summary>
    /// 获取变更的实体属性内容
    /// </summary>
    public void GetChangedProperties()
    {
        var currentEntity = dbContext.EntityInfos?.Query()
            .Where(e => e.Name == EntityInfo.Name
                && e.NamespaceName == EntityInfo.NamespaceName
                && e.ProjectId == Const.PROJECT_ID)
            .FirstOrDefault();

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
                    Type = ChangeType.Update,
                    Origin = p.ToCsharpLine(true),
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
            dbContext.EntityInfos?.Delete(currentEntity.Id);
        }
        dbContext.EntityInfos?.EnsureIndex(e => e.Name);
        dbContext.EntityInfos?.Insert(EntityInfo);
        PropertyChanges.ForEach(p =>
        {
            Console.WriteLine(p.Type.ToString() + " : " + p.Name);
        });
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
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.ShortDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "概要"),
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?
                .Where(p => p.Name != "IsDeleted")
                .Where(p => !p.IsJsonIgnore)
                .ToList()
        };

        dto.Properties = dto.Properties?.Where(
            p => p.Name != "Content"
                && p.Name != "UpdatedTime"
                && p.Name != "CreatedTime"
                && p.MaxLength < 1000
                && !p.Name.EndsWith("Id") && p.Name != "Id"
                && !(p.IsList && p.IsNavigation)
            )
            .ToList();

        return dto.ToDtoContent(AssemblyName, EntityInfo.Name);
    }

    public string? GetItemDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.ItemDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "列表元素"),
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?
                .Where(p => p.Name is not "IsDeleted" and not "UpdatedTime")
                .Where(p => !p.IsJsonIgnore)
                .ToList()
        };

        dto.Properties = dto.Properties?
            .Where(p => !p.IsList
                && p.MaxLength < 1000
                && !p.Name.EndsWith("Id") && p.Name != "Id"
                && !p.IsNavigation).ToList();

        return dto.ToDtoContent(AssemblyName, EntityInfo.Name);
    }

    public string? GetFilterDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }
        List<PropertyInfo>? referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList)
            .Where(p => !p.IsJsonIgnore)
            .Select(s => new PropertyInfo()
            {
                Name = s.Name + "Id",
                Type = KeyType + "?",
                ProjectId = Const.PROJECT_ID
            })
            .ToList();

        string[] filterFields = ["Id", "CreatedTime", "UpdatedTime", "IsDeleted", "PageSize", "PageIndex"];

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.FilterDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "查询筛选"),
            Tag = EntityInfo.Name,
            BaseType = "FilterBase",
        };

        List<PropertyInfo>? properties = EntityInfo.PropertyInfos?
                .Where(p => (p.IsRequired && !p.IsNavigation)
                    || (!p.IsList
                        && !p.IsNavigation
                        && !p.IsComplexType
                        && !filterFields.Contains(p.Name))
                     || p.IsEnum
                    )
                .Where(p => p.MaxLength is not (not null and >= 1000))
                .ToList();
        dto.Properties = properties.Copy() ?? [];

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
        return dto.ToDtoContent(AssemblyName, EntityInfo.Name);
    }

    public string? GetAddDto()
    {
        if (EntityInfo == null)
        {
            return default;
        }

        List<PropertyInfo>? referenceProps = EntityInfo.PropertyInfos?
            .Where(p => !p.IsJsonIgnore)
            .Where(p => p.IsNavigation &&
                (p.IsRequired || !p.IsNullable || !string.IsNullOrWhiteSpace(p.DefaultValue)))
            .Where(p => !p.Type.Equals("User") && !p.Type.Equals("SystemUser"))
            .Select(s => new PropertyInfo()
            {
                Name = s.NavigationName + (s.IsList ? "Ids" : "Id"),
                Type = s.IsList ? $"List<{KeyType}>" + (s.IsRequired ? "" : "?") : KeyType,
                IsRequired = s.IsRequired,
                IsNullable = s.IsNullable,
                DefaultValue = "",
                ProjectId = Const.PROJECT_ID
            })
            .ToList();

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.AddDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "添加时请求结构"),
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?.Where(p => !p.IsNavigation
                && p.HasSet
                && p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && p.Name != "IsDeleted")
            .ToList() ?? []
        };

        // 初次创建
        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });


        return dto.ToDtoContent(AssemblyName, EntityInfo.Name, true);
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
            .Where(p => !p.IsJsonIgnore)
            .Where(p => p.IsNavigation &&
                (p.IsRequired || !p.IsNullable || !string.IsNullOrWhiteSpace(p.DefaultValue)))
            .Where(p => !p.Type.Equals("User") && !p.Type.Equals("SystemUser"))
            .Select(s => new PropertyInfo()
            {
                Name = s.NavigationName + (s.IsList ? "Ids" : "Id"),
                Type = s.IsList ? $"List<{KeyType}>" : KeyType,
                ProjectId = Const.PROJECT_ID,
                IsRequired = s.IsRequired,
                IsNullable = s.IsNullable,
            })
            .ToList();

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.UpdateDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "更新时请求结构"),
            Tag = EntityInfo.Name,

        };
        // 处理非required的都设置为nullable
        List<PropertyInfo>? properties = EntityInfo.PropertyInfos?.Where(p => !p.IsNavigation
                && p.HasSet
                && p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && p.Name != "IsDeleted")
            .ToList();

        dto.Properties = properties?.Copy() ?? [];

        referenceProps?.ForEach(item =>
        {
            if (!dto.Properties.Any(p => p.Name.Equals(item.Name)))
            {
                dto.Properties.Add(item);
            }
        });

        foreach (PropertyInfo item in dto.Properties)
        {
            item.IsNullable = true;
        }
        return dto.ToDtoContent(AssemblyName, EntityInfo.Name);
    }

    public string GetDtoUsings()
    {
        return $"""
        global using System;
        global using System.ComponentModel.DataAnnotations;
        global using {AssemblyName}.Models;
        global using Ater.Web.Core.Models;
        global using Entity;
        """;
    }
}
