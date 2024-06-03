using System.Text.Json;
using System.Text.Json.Serialization;

namespace Definition.Infrastructure.Utils;

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
