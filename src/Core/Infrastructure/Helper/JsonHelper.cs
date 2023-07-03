using System.Text.Json.Nodes;

namespace Core.Infrastructure.Helper;
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
        var paths = keyPath.Split('.');
        var current = root;
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
                    current.AsObject()!.Append(new KeyValuePair<string, JsonNode>(paths[i], ""));
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
}
