using System.Text.Json.Nodes;

namespace Share.Infrastructure.Helper;
/// <summary>
/// json帮助类
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// 添加或更新json节点
    /// </summary>
    /// <param name="root"></param>
    /// <param name="keyPath"></param>
    /// <param name="newValue"></param>
    public static void AddOrUpdateJsonNode(JsonNode root, string keyPath, object newValue)
    {
        string[] paths = keyPath.Split('.');
        JsonNode? current = root;
        if (current == null) return;
        try
        {
            for (int i = 0; i < paths.Length - 1; i++)
            {
                if (current!.AsObject().ContainsKey(paths[i]))
                {
                    current = current[paths[i]];
                }
                else
                {
                    // add new node with path 
                    current.AsObject()!.Append(new KeyValuePair<string, JsonNode?>(paths[i], ""));
                    current = current[paths[i]];
                }

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

    /// <summary>
    /// 获取值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="node"></param>
    /// <param name="keyPath"></param>
    /// <returns></returns>
    public static T? GetValue<T>(JsonNode node, string keyPath) where T : class
    {
        string[] paths = keyPath.Split('.');
        JsonNode? current = node;
        if (current == null) return default;

        for (int i = 0; i < paths.Length; i++)
        {
            if (current!.AsObject().ContainsKey(paths[i]))
            {
                current = current[paths[i]];
            }
            else
            {
                return default;
            }
        }
        return current!.GetValue<T>();
    }

    /// <summary>
    /// 获取节点
    /// </summary>
    /// <param name="root"></param>
    /// <param name="keyPath"></param>
    /// <returns></returns>
    public static JsonNode? GetSectionNode(JsonNode root, string keyPath)
    {
        string[] paths = keyPath.Split('.');
        JsonNode? current = root;
        if (current == null) return default;

        for (int i = 0; i < paths.Length; i++)
        {
            if (current!.AsObject().ContainsKey(paths[i]))
            {
                current = current[paths[i]];
            }
            else
            {
                return default;
            }
        }
        return current;
    }
}
