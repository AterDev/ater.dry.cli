namespace GeneratorForNode.Utils;

public static class StringExtensions
{
    /// <summary>
    /// FromBase64
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string? FromBase64String(this string str)
    {
        byte[] buffer = new byte[str.Length * 3 / 4];
        if (Convert.TryFromBase64String(str, buffer, out int bytesWritten))
        {
            return Encoding.UTF8.GetString(buffer, 0, bytesWritten);
        }
        else
        {
            return null;
        }
    }
    public static bool NotNull(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }
    /// <summary>
    /// to hyphen style: HelloWord->hello-word
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
    /// to snake lower style: HelloWord->hello_word
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToSnakeLower(this string str)
    {
        return str.ToHyphen('_');
    }

    /// <summary>
    /// to Pascalcase style:hello-word->HelloWord
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
        foreach (var c in str)
        {
            _ = !char.IsLetterOrDigit(c) ? resultBuilder.Append(' ') : resultBuilder.Append(c);
        }
        var result = resultBuilder.ToString();
        result = string.Join(string.Empty, result.Split(' ').Select(r => r.ToUpperFirst()).ToArray());
        return result;
    }

    /// <summary>
    /// to camelcase style:hello-word->hellowWord
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
