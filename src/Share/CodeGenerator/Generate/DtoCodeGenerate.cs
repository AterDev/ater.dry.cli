using System.Text.RegularExpressions;

using Mapster;

using PropertyInfo = Entity.PropertyInfo;

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
    public string? AssemblyName { get; set; }
    public string DtoPath { get; init; }

    public DtoCodeGenerate(EntityInfo entityInfo, string dtoPath)
    {
        DtoPath = dtoPath;
        AssemblyName = AssemblyHelper.GetNamespaceName(new DirectoryInfo(dtoPath));
        EntityInfo = entityInfo;
        KeyType = EntityInfo.KeyType switch
        {
            EntityKeyType.Int => "Int",
            EntityKeyType.String => "String",
            _ => "Guid"
        };
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

    /// <summary>
    /// the detail dto
    /// </summary>
    /// <returns></returns>
    public string GetDetailDto()
    {
        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.ShortDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "概要"),
            Tag = EntityInfo.Name,
            Properties = EntityInfo.PropertyInfos?
                .Where(p => p.Name is not "IsDeleted")
                .Where(p => !p.IsJsonIgnore)
                .Where(p => !EntityInfo.IgnoreTypes.Contains(p.Type))
                .Where(p => !(p.IsList && p.IsNavigation))
                .ToList() ?? []
        };

        return dto.ToDtoContent(AssemblyName, EntityInfo.Name);
    }

    /// <summary>
    /// the list item dto
    /// </summary>
    /// <returns></returns>
    public string GetItemDto()
    {
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
                .ToList() ?? []
        };

        dto.Properties = dto.Properties?
            .Where(p => !p.IsList
                && (p.MaxLength is not (not null and >= 200))
                && (!p.Name.EndsWith("Id") || p.Name.Equals("Id"))
                && !p.IsNavigation).ToList() ?? [];

        return dto.ToDtoContent(AssemblyName, EntityInfo.Name);
    }

    /// <summary>
    /// the filter dto
    /// </summary>
    /// <returns></returns>
    public string GetFilterDto()
    {
        List<PropertyInfo>? referenceProps = EntityInfo.PropertyInfos?
            .Where(p => p.IsNavigation && !p.IsList)
            .Where(p => !p.IsJsonIgnore)
            .Select(s => new PropertyInfo()
            {
                Name = s.Name + "Id",
                Type = KeyType + "?",
            })
            .ToList();

        DtoInfo dto = new()
        {
            EntityNamespace = $"{EntityInfo.NamespaceName}.{EntityInfo.Name}",
            Name = EntityInfo.Name + Const.FilterDto,
            NamespaceName = EntityInfo.NamespaceName,
            Comment = FormatComment(EntityInfo.Comment, "查询筛选"),
            Tag = EntityInfo.Name,
            BaseType = "FilterBase",
            Properties = EntityInfo.GetFilterProperties()
                .Select(p => p.Adapt<PropertyInfo>())
                .ToList() ?? []
        };

        // 筛选条件调整为可空
        foreach (PropertyInfo item in dto.Properties)
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

    public string GetAddDto()
    {
        // 导航属性处理
        List<PropertyInfo>? referenceProps = EntityInfo.PropertyInfos?
            .Where(p => !p.IsJsonIgnore && !p.IsList)
            .Where(p => p.IsNavigation &&
                (p.IsRequired || !p.IsNullable || !string.IsNullOrWhiteSpace(p.DefaultValue)))
            .Where(p => !p.Type.Equals("User") && !p.Type.Equals("SystemUser"))
            .Select(s => new PropertyInfo()
            {
                Name = s.NavigationName + "Id",
                Type = s.IsList ? $"List<{KeyType}>" + (s.IsRequired ? "" : "?") : KeyType,
                IsRequired = s.IsRequired,
                IsNullable = s.IsNullable,
                DefaultValue = "",
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
                && !EntityInfo.IgnoreTypes.Contains(p.Type)
                && p.Name != "Id"
                && p.Name != "CreatedTime"
                && p.Name != "UpdatedTime"
                && p.Name != "IsDeleted")
            .ToList() ?? []
        };

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
    public string GetUpdateDto()
    {
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
            // 处理非 required的都设置为 nullable
            Properties = EntityInfo.PropertyInfos?.Where(p => !p.IsNavigation
                    && p.HasSet
                    && !EntityInfo.IgnoreTypes.Contains(p.Type)
                    && p.Name != "Id"
                    && p.Name != "CreatedTime"
                    && p.Name != "UpdatedTime"
                    && p.Name != "IsDeleted")
            .Select(p => p.Adapt<PropertyInfo>())
            .ToList() ?? []
        };

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

    public List<string> GetGlobalUsings()
    {
        return [
        "global using System;",
        "global using System.Text.Json;",
        "global using System.ComponentModel.DataAnnotations;",
        $"global using {AssemblyName}.Models;",
        "global using Ater.Web.Core.Models;",
        $"global using {EntityInfo.NamespaceName};"
        ];
    }
}
