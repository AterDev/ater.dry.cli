using PropertyInfo = CodeGenerator.Models.PropertyInfo;

namespace Droplet.CommandLine.Commands;

/// <summary>
/// 表单生成
/// </summary>
public class NgFormGenerate : GenerateBase
{
    public NgFormGenerate()
    {
    }

    public static void Add(PropertyInfo input)
    {
    }

    /// <summary>
    /// 生成添加组件
    /// </summary>
    public static string GenerateAddForm(List<PropertyInfo>? propertyInfos)
    {
        var formControls = "";
        if (propertyInfos != null)
            foreach (var input in propertyInfos)
            {
                formControls += input.ToNgInputControl();
            }
        var tplContent = GetTplContent("angular.add.add.component.html.tpl");
        tplContent = tplContent.Replace("{$FormControls}", formControls);
        return tplContent;
    }

    public static string GenerateEditForm(List<PropertyInfo>? propertyInfos)
    {
        var formControls = "";
        if (propertyInfos != null)
            foreach (var input in propertyInfos)
            {
                formControls += input.ToNgInputControl();
            }
        var tplContent = GetTplContent("angular.edit.edit.component.html.tpl");
        tplContent = tplContent.Replace("{$FormControls}", formControls);
        return tplContent;
    }

}
