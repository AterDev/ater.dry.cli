using Definition.Infrastructure.Helper;

namespace AterStudio.Manager;

/// <summary>
/// 工具类
/// </summary>
public class ToolsManager
{

    public ToolsManager()
    {

    }


    public List<string>? ConvertToClass(string json)
    {
        if (CSharpCovertHelper.CheckJson(json))
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            var helper = new CSharpCovertHelper();
            helper.GenerateClass(jsonElement);
            return helper.ClassCodes;
        }
        return null;
    }
}
