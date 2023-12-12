using System.Text.Json;

namespace Core.Infrastructure.Helper;
/// <summary>
/// csharp 转换帮助
/// </summary>
public class CSharpCovertHelper
{
    public List<string> ClassCodes { get; set; } = [];

    /// <summary>
    /// 判断是否为合法的 json
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static bool CheckJson(string json)
    {
        try
        {
            var obj = JsonSerializer.Deserialize<object>(json);
            return obj != null;
        }
        catch (Exception)
        {
            return false;
        }
    }


    /// <summary>
    /// json转C#模型类
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    public void GenerateClass(JsonElement jsonElement, string className = "Model")
    {
        className = className.ToPascalCase();
        var sb = new StringBuilder();
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");
        if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            jsonElement = jsonElement.EnumerateArray().First();
        }

        foreach (var property in jsonElement.EnumerateObject())
        {
            var propertyName = property.Name;
            var propertyValue = property.Value;
            var defaultValue = "";
            // 类型处理
            string csharpType = propertyValue.ValueKind switch
            {
                JsonValueKind.Number => "int",
                JsonValueKind.String => "string?",
                JsonValueKind.True => "bool",
                JsonValueKind.False => "bool",
                JsonValueKind.Object => propertyName.ToPascalCase(),
                JsonValueKind.Array => $"List<{propertyName.ToPascalCase()}>",
                JsonValueKind.Null => "object?",
                _ => "object",
            };
            if (propertyValue.ValueKind == JsonValueKind.Number)
            {
                if (propertyValue.TryGetDouble(out _))
                {
                    csharpType = "double";
                }

                if (propertyValue.TryGetInt32(out _))
                {
                    csharpType = "int";
                }

                if (propertyValue.TryGetInt64(out _))
                {
                    csharpType = "long";
                }
            }
            if (propertyValue.ValueKind == JsonValueKind.String)
            {
                if (propertyValue.TryGetGuid(out _))
                {
                    csharpType = "Guid";
                }
                if (propertyValue.TryGetDateTime(out _))
                {
                    csharpType = "DateTime";
                }
                if (propertyValue.TryGetDateTimeOffset(out DateTimeOffset dateTimeOffset))
                {
                    csharpType = "DateTimeOffset";
                    // if this is a DateOnly
                    if (dateTimeOffset.TimeOfDay == TimeSpan.Zero)
                    {
                        csharpType = "DateOnly";
                    }
                }
                var stringValue = propertyValue.GetString();
                string[] formats = ["HH:mm:ss", "HH:mm"];
                if (DateTime.TryParseExact(stringValue, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
                {
                    csharpType = "TimeOnly";
                }
                formats = ["yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss"];
                if (DateTime.TryParseExact(stringValue, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
                {
                    csharpType = "DateTime";
                }

            }
            if (propertyValue.ValueKind == JsonValueKind.Array)
            {
                defaultValue = "[]";
            }

            if (propertyName.ToUpperFirst() != propertyName.ToPascalCase())
            {
                sb.AppendLine($"    [JsonPropertyName(\"{propertyName}\")]");
            }

            // 处理非法类型或变量名称
            if (int.TryParse(csharpType, out _))
            {
                csharpType = "The" + csharpType;
            }
            if (int.TryParse(propertyName, out _))
            {
                propertyName = "The" + propertyName;
            }

            var propertyLine = $"public {csharpType} {propertyName.ToPascalCase()} {{ get; set; }}";
            if (!string.IsNullOrEmpty(defaultValue))
            {
                propertyLine += $" = {defaultValue};";
            }
            sb.AppendLine($"    {propertyLine}");

            // 对象处理
            if (propertyValue.ValueKind == JsonValueKind.Object)
            {
                GenerateClass(propertyValue, propertyName);
            }
            // 数组处理
            else if (propertyValue.ValueKind == JsonValueKind.Array && propertyValue.GetArrayLength() > 0
                && propertyValue[0].ValueKind == JsonValueKind.Object)
            {
                GenerateClass(propertyValue[0], propertyName);
            }
        }
        sb.AppendLine("}");
        ClassCodes.Add(sb.ToString());
    }
}
/// <summary>
/// json 解析的元信息
/// </summary>
internal class JsonMetadata
{
    public List<JsonMetadata> Descents { get; set; } = [];

    public JsonMetadata? Parent { get; set; }

    public JsonMetadataType Type { get; set; }

    public enum JsonMetadataType
    {
        Object,
        Array,
        Dictionary,
        KeyValue
    }
}