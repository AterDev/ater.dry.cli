using System.Text.Json.Nodes;

namespace Core.Infrastructure.Helper;
/// <summary>
/// json帮助类
/// </summary>
public static class JsonHelper
{
    public static void UpdateJsonNode(JsonNode root, string keyPath, object newValue)
    {
        var paths = keyPath.Split('.');
        var current = root;

        try
        {
            for (int i = 0; i < paths.Length - 1; i++)
            {
                current = current?[paths[i]];
            }
            if (current != null)
            {
                current[paths[^1]] = JsonValue.Create(newValue);
            }
        }
        catch (Exception)
        {
        }
    }
}
