using System.Text;

namespace AterStudio.Manager;

public class ToolsManager
{

    public ToolsManager()
    {

    }



    /// <summary>
    /// 判断是否为合法的json
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public bool CheckJson(string json)
    {
        return true;
    }


    /// <summary>
    /// json转C#模型类
    /// </summary>
    /// <param name="json"></param>
    /// <param name="jsonElement"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    public string GenerateClass(string json, JsonElement jsonElement, string className = "Model")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");

        foreach (var property in jsonElement.EnumerateObject())
        {
            var propertyName = property.Name;
            var propertyValue = property.Value;

            string csharpValue = string.Empty;
        }
        return "";
    }

}
