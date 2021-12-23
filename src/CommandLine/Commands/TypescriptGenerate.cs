
using CodeGenerator.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
namespace Droplet.CommandLine.Commands;

/// <summary>
/// generate typescript model file
/// </summary>
public class TypescriptGenerate
{
    public IDictionary<string, OpenApiSchema> Schemas { get; set; }
    public TypescriptGenerate()
    {
    }
    public TypescriptGenerate(IDictionary<string, OpenApiSchema> schemas)
    {
        Schemas = schemas;
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
        string res = "";
        string comment = "";
        string propertyString = "";
        string extendString = "";
        string importString = "";// 需要导入的关联接口
                                 // 处理继承的情况
        if (schema.AllOf.Count > 0)
        {
            var extend = schema.AllOf.First()?.Reference?.Id;
            if (!string.IsNullOrEmpty(extend))
            {
                extendString = "extends " + extend + " ";
                // 如果是自引用，不需要导入
                if (extend != name)
                {
                    importString += @$"import {{ {extend} }} from './{extend.ToHyphen()}.model';" + Environment.NewLine;
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
            propertyString += p.ToString();
        });
        // 去重
        var importsProps = props.Where(p => !string.IsNullOrEmpty(p.Reference))
            .Select(p => p.Reference)
            .Distinct()
            .ToList();
        importsProps.ForEach(ip =>
        {
                // 引用的导入，自引用不需要导入
                if (ip != name)
            {
                importString += @$"import {{ {ip} }} from './{ip.ToHyphen()}.model';" + Environment.NewLine;
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
    public string ToEnumString(OpenApiSchema schema, string name = "")
    {
        string res = "";
        string comment = "";
        string propertyString = "";
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

        for (int i = 0; i < values.Count; i++)
        {
            propertyString += "  " + (values[i] as OpenApiString).Value + " = " + i + ",\n";
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
    public List<TsProperty> GetTsProperties(OpenApiSchema schema)
    {
        var tsProperties = new List<TsProperty>();
        // 继承的需要递归 从AllOf中获取属性
        if (schema.AllOf.Count > 1)
        {
            // 自己的属性在1中
            tsProperties.AddRange(GetTsProperties(schema.AllOf[1]));
        }
        if (schema.Properties.Count > 0)
        {
            // TODO:泛型处理
            foreach (var prop in schema.Properties)
            {
                string type = GetTsType(prop.Value);
                string propComments = "";
                string name = prop.Key;

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
                {
                    refType = prop.Value.Items;
                }
                if (prop.Value.Items?.OneOf.Count > 0)
                {
                    refType = prop.Value.Items.OneOf.FirstOrDefault();
                }
                if (refType?.Reference != null)
                {
                    property.Reference = refType.Reference.Id;
                }
                if (prop.Value.Reference != null)
                {
                    property.Reference = prop.Value.Reference.Id;
                }
                // 可空处理
                tsProperties.Add(property);
            }
        }
        // 重写的属性去重
        var res = tsProperties.GroupBy(p => p.Name).Select(s => s.FirstOrDefault()).ToList();
        return res;
    }

    /// <summary>
    /// 获取转换成ts的类型
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public string GetTsType(OpenApiSchema prop)
    {
        string type = "string";
        // 常规类型
        switch (prop.Type)
        {
            case "boolean":
                type = "boolean";
                break;

            case "integer":
                // 看是否为enum
                if (prop.Enum.Count > 0)
                {
                    type = prop.Reference?.Id ?? "";
                }
                else
                {
                    type = "number";
                }
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

                if (prop.Items.Reference != null)
                {
                    type = prop.Items.Reference.Id + "[]";
                }
                else
                {
                    type = GetTsType(prop.Items) + "[]";
                }

                break;
            default:
                break;
        }
        // 引用对象
        if (prop.OneOf.Count > 0)
        {
            // 获取引用对象名称
            type = prop.OneOf.First()?.Reference.Id;
        }
        if (prop.Nullable || prop.Reference != null)
        {
            type += " | null";
        }
        return type;
    }

    /// <summary>
    /// 生成文件
    /// </summary>
    /// <param name="ngRootPath">angular root dir path</param>
    public async Task BuildInterfaceAsync(string ngRootPath = "")
    {
        if (string.IsNullOrEmpty(ngRootPath))
        {
            var parentPath = new DirectoryInfo(Environment.CurrentDirectory).Parent.FullName;
            ngRootPath = Path.Combine(parentPath, "angluar");
        }
        // 生成文件的目录名称
        var modelsPath = Path.Combine(ngRootPath, "src", "app", "share", "models");
        if (!Directory.Exists(modelsPath))
        {
            Directory.CreateDirectory(modelsPath);
        }

        foreach (var schema in Schemas)
        {
            // 文件名及内容
            var fileName = schema.Key.ToHyphen() + ".model.ts";
            string tsContent;
            if (schema.Value.Enum.Count > 0)
            {
                tsContent = ToEnumString(schema.Value, schema.Key);
            }
            else
            {
                tsContent = ToInterfaceString(schema.Value, schema.Key);
            }
            // 写入文件
            await File.WriteAllTextAsync(Path.Combine(modelsPath, fileName), tsContent);
        }
    }
}

public class TsProperty
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string Reference { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public string? Comments { get; set; }

    public override string ToString()
    {
        var name = Name + (IsNullable ? "?: " : ": ");
        // 引用的类型可空
        if (!string.IsNullOrEmpty(Reference))
        {
            name = Name + "?: ";
        }
        return $"{Comments}  {name}{Type};\n";
    }
}
