using System.Text.Json;
using System.Text.Json.Serialization;

namespace Definition.Infrastructure.Utils;

public static class StringExtenstions
{
    /// <summary>
    /// to hyphen style: HelloWord->hellow-word
    /// </summary>
    /// <param name="str"></param>
    /// <param name="separator">分隔符</param>
    /// <returns></returns>
    public static string ToHyphen(this string str, char separator = '-')
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        int upperNumber = 0;
        for (int i = 0; i < str.Length; i++)
        {
            char item = str[i];
            // 连续的大写只添加一个-
            char pre = i >= 1 ? str[i - 1] : 'a';
            if (char.IsUpper(item) && char.IsLower(pre))
            {
                upperNumber++;
                if (upperNumber > 1)
                {
                    _ = builder.Append(separator);
                }
            }
            else if (item is '_' or ' ')
            {
                _ = builder.Append(separator);
            }
            _ = builder.Append(char.ToLower(item));
        }
        return builder.ToString();
    }

    /// <summary>
    /// to Pascalcase style:hellow-word->HelloWord
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }
        StringBuilder resultBuilder = new();
        foreach (char c in str)
        {
            _ = !char.IsLetterOrDigit(c) ? resultBuilder.Append(' ') : resultBuilder.Append(c);
        }
        string result = resultBuilder.ToString();
        result = string.Join(string.Empty, result.Split(' ').Select(r => r.ToUpperFirst()).ToArray());
        return result;
    }

    /// <summary>
    /// to camelcase style:hellow-word->hellowWord
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }
        str = str.ToPascalCase();
        return char.ToLower(str[0]) + str[1..];
    }
    public static string ToUpperFirst(this string str)
    {
        return string.IsNullOrWhiteSpace(str) ? string.Empty : char.ToUpper(str[0]) + str[1..];
    }

    public static bool NotNull(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static T? Copy<T>(this T origin)
    {
        if (origin == null)
        {
            return default;
        }

        MemoryStream stream = new();
        var jsonOption = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        JsonSerializer.Serialize(stream, origin, jsonOption);
        stream.Position = 0;
        return JsonSerializer.Deserialize<T>(stream, jsonOption);
    }
}
