using System.Text.Json;

namespace Core.Infrastructure.Helper;
/// <summary>
/// csharp 转换帮助
/// </summary>
public class CSharpCovertHelper
{
    public List<string> ClassCodes { get; set; } = new();

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

            string csharpValue = propertyValue.ValueKind switch
            {
                JsonValueKind.Number => "int",
                JsonValueKind.String => "required string",
                JsonValueKind.True => "bool",
                JsonValueKind.False => "bool",
                JsonValueKind.Object => propertyName,
                JsonValueKind.Array => $"List<{propertyName}>?",
                JsonValueKind.Null => "object?",
                _ => "object",
            };
            if (propertyValue.ValueKind == JsonValueKind.Number)
            {

                if (propertyValue.TryGetDouble(out var doubleValue))
                {
                    csharpValue = "double";
                }
                if (propertyValue.TryGetInt32(out var intValue))
                {
                    csharpValue = "int";
                }
                if (propertyValue.TryGetInt64(out var longValue))
                {
                    csharpValue = "long";
                }
            }
            if (propertyValue.ValueKind == JsonValueKind.String)
            {
                if (propertyValue.TryGetGuid(out var stringValue))
                {
                    csharpValue = "Guid";
                }
            }

            if (propertyName != propertyName.ToPascalCase())
            {
                sb.AppendLine($"    [JsonPropertyName(\"{propertyName}\")]");
            }
            sb.AppendLine($"    public {csharpValue} {propertyName.ToPascalCase()} {{ get; set; }}");
            if (propertyValue.ValueKind == JsonValueKind.Object)
            {
                GenerateClass(propertyValue, propertyName);
            }
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
