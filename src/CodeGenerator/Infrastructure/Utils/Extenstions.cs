using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace CodeGenerator.Infrastructure.Utils;

public static class Extenstions
{
    /// <summary>
    /// to hyphen style: HelloWord->hellow-word
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToHyphen(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        var upperNumber = 0;
        for (var i = 0; i < str.Length; i++)
        {
            var item = str[i];
            // 连续的大写只添加一个-
            var pre = i >= 1 ? str[i - 1] : 'a';
            if (char.IsUpper(item) && char.IsLower(pre))
            {
                upperNumber++;
                if (upperNumber > 1)
                {
                    builder.Append('-');
                }
            }
            else if (item == '_' || item == ' ')
            {
                builder.Append('-');
            }
            builder.Append(char.ToLower(item));
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
        var resultBuilder = new StringBuilder();
        foreach (var c in str)
        {
            if (!char.IsLetterOrDigit(c))
            {
                resultBuilder.Append(' ');
            }
            else
            {
                resultBuilder.Append(c);
            }
        }
        var result = resultBuilder.ToString();
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
        return char.ToLower(str[0]) + str.Substring(1);
    }
    public static string ToUpperFirst(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return string.Empty;
        }
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    public static bool NotNull(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static T? Copy<T>(this T origin)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, origin);
        stream.Position = 0;
        return JsonSerializer.Deserialize<T>(stream);
    }
}
