using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Xml.Linq;

namespace CodeGenerator.Generate;
/// <summary>
/// generate typescript model file
/// </summary>
public class TSModelGenerate : GenerateBase
{
    public Dictionary<string, string?> ModelDictionary { get; set; }
    public TSModelGenerate(Dictionary<string, string?> modelDictionary)
    {
        ModelDictionary = modelDictionary;
    }

    /// <summary>
    /// 生成ts interface
    /// </summary>
    /// <param name="dir">目录 ,apiTag</param>
    /// <param name="typeRefName">ref对应的名称</param>
    /// <returns></returns>
    public GenFileInfo GenerateInterfaceFile(string schemaKey, OpenApiSchema schema)
    {
        // 文件名及内容
        var fileName = schemaKey.ToHyphen() + ".model.ts";
        string tsContent;
        ModelDictionary.TryGetValue(schemaKey, out var path);
        if (schema.Enum.Count > 0)
        {
            tsContent = ToEnumString(schema, schemaKey);
            path = "enum";
        }
        else
        {
            tsContent = ToInterfaceString(schema, schemaKey);
        }
        var file = new GenFileInfo(tsContent)
        {
            Name = fileName,
            Path = path??"",
            Content = tsContent

        };
        return file;
    }

    /// <summary>
    /// 将 Schemas 转换成 ts 接口
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="name"></param>
    /// <param name="onlyProps"></param>
    /// <returns></returns>
    public string ToInterfaceString(OpenApiSchema schema, string name = "", bool onlyProps = false)
    {
        var res = "";
        var comment = "";
        var propertyString = "";
        var extendString = "";
        var importString = "";// 需要导入的关联接口
        var relatePath = "../";

        // 不在tags里的默认的根目录
        if (ModelDictionary.ContainsKey(name) && ModelDictionary[name] == null)
        {
            relatePath = "./";
        }
        if (schema.AllOf.Count > 0)
        {
            var extend = schema.AllOf.First()?.Reference?.Id;
            if (!string.IsNullOrEmpty(extend))
            {
                extendString = "extends " + extend + " ";
                // 如果是自引用，不需要导入
                if (extend != name)
                {
                    ModelDictionary.TryGetValue(extend, out var dirName);
                    dirName = dirName.NotNull() ? dirName!.ToHyphen() + "/" : "";
                    importString += @$"import {{ {extend} }} from '{relatePath}{dirName}{extend.ToHyphen()}.model';"
                        + Environment.NewLine;
                }
            }
        }
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment = @$"/**
 * {schema.Description}
 */
";
        }

        var props = GetTsProperties(schema);
        props.ForEach(p =>
        {
            propertyString += p.ToProperty();
        });
        // 去重
        var importsProps = props.Where(p => !string.IsNullOrEmpty(p.Reference))
            .Distinct()
            .ToList();
        importsProps.ForEach(ip =>
        {
            // 引用的导入，自引用不需要导入
            if (ip.Reference != name)
            {
                ModelDictionary.TryGetValue(name, out var dirName);
                dirName = dirName.NotNull() ? dirName!.ToHyphen() + "/" : "";
                if (ip.IsEnum) dirName = "enum/";
                importString += @$"import {{ {ip.Reference} }} from '{relatePath}{dirName}{ip.Reference.ToHyphen()}.model';"
                + Environment.NewLine;
            }
        });

        res = @$"{importString}{comment}export interface {name} {extendString}{{
{propertyString}
}}
";
        return res;
    }


    /// <summary>
    /// 生成enum
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ToEnumString(OpenApiSchema schema, string name = "")
    {
        var res = "";
        var comment = "";
        var propertyString = "";
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment = @$"/**
 * {schema.Description}
 */
";
        }
        var enumNames = schema.Extensions
            .Where(e => e.Key == "x-enumNames")
            .First();

        var values = enumNames.Value as OpenApiArray;

        for (var i = 0; i < values?.Count; i++)
        {
            propertyString += "  " + ((OpenApiString)values[i]).Value + " = " + i + ",\n";
        }
        res = @$"{comment}export enum {name} {{
{propertyString}
}}
";
        return res;
    }

    /// <summary>
    /// 获取所有属性
    /// </summary>
    /// <param name="schema"></param>
    /// <returns></returns>
    public static List<TsProperty> GetTsProperties(OpenApiSchema schema)
    {
        var tsProperties = new List<TsProperty>();
        // 继承的需要递归 从AllOf中获取属性
        if (schema.AllOf.Count > 1)
            // 自己的属性在1中
            tsProperties.AddRange(GetTsProperties(schema.AllOf[1]));
        if (schema.Properties.Count > 0)
        {
            // TODO:泛型处理
            foreach (var prop in schema.Properties)
            {
                var type = GetTsType(prop.Value);
                var propComments = "";
                var name = prop.Key;

                if (!string.IsNullOrEmpty(prop.Value.Description))
                {
                    propComments = @$"  /**
   * {prop.Value.Description}
   */
";
                }
                var property = new TsProperty
                {
                    Comments = propComments,
                    IsNullable = prop.Value.Nullable,
                    Name = name,
                    Type = type
                };
                // 是否是关联属性
                var refType = prop.Value.OneOf?.FirstOrDefault();
                // 列表中的类型
                if (prop.Value.Items?.Reference != null)
                    refType = prop.Value.Items;
                if (prop.Value.Items?.OneOf.Count > 0)
                    refType = prop.Value.Items.OneOf.FirstOrDefault();
                if (refType?.Reference != null)
                    property.Reference = refType.Reference.Id;
                if (prop.Value.Reference != null)
                    property.Reference = prop.Value.Reference.Id;
                if (prop.Value.Enum.Any() ||
                    (refType != null && refType.Enum.Any()))
                    property.IsEnum = true;

                // 可空处理
                tsProperties.Add(property);
            }
        }
        // 重写的属性去重
        var res = tsProperties.GroupBy(p => p.Name)
            .Select(s => s.FirstOrDefault()).ToList();
        return res!;
    }

    /// <summary>
    /// 获取转换成ts的类型
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static string GetTsType(OpenApiSchema prop)
    {
        var type = "string";
        // 常规类型
        switch (prop.Type)
        {
            case "boolean":
                type = "boolean";
                break;

            case "integer":
                // 看是否为enum
                type = prop.Enum.Count > 0
                    ? prop.Reference?.Id
                    : "number";
                break;
            case "number":
                type = "number";
                break;
            case "string":
                switch (prop.Format)
                {
                    case "guid":
                        break;
                    case "binary":
                        type = "formData";
                        break;
                    case "date-time":
                        type = "Date";
                        break;
                    default:
                        type = "string";
                        break;
                }
                break;
            case "array":
                type = prop.Items.Reference != null
                    ? prop.Items.Reference.Id + "[]"
                    : GetTsType(prop.Items) + "[]";
                break;
            default:
                type = prop.Reference?.Id;
                break;
        }
        // 引用对象
        if (prop.OneOf.Count > 0)
            // 获取引用对象名称
            type = prop.OneOf.First()?.Reference.Id;
        if (prop.Nullable || prop.Reference != null)
            type += " | null";
        return type ?? "string";
    }
}

public class TsProperty
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string Reference { get; set; } = string.Empty;
    public bool IsEnum { get; set; } = false;
    public bool IsNullable { get; set; } = false;
    public string? Comments { get; set; }

    public string ToProperty()
    {
        var name = Name + (IsNullable ? "?: " : ": ");
        // 引用的类型可空
        if (!string.IsNullOrEmpty(Reference))
            name = Name + "?: ";
        return $"{Comments}  {name}{Type};" + Environment.NewLine;
    }
}
