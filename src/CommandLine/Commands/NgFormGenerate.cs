namespace Droplet.CommandLine.Commands;

/// <summary>
/// 表单生成
/// </summary>
public class NgFormGenerate : GenerateBase
{
    public NgFormGenerate()
    {
    }

    public void Add(PropertyInfo input)
    {
    }

    /// <summary>
    /// 生成添加组件
    /// </summary>
    public string GenerateAddForm(List<PropertyInfo> propertyInfos, string name)
    {
        var formControls = "";
        foreach (var input in propertyInfos)
        {
            formControls += input.ToNgInputControl();
        }
        var tplContent = GetTplContent("NgViews.add.add.component.html.tpl");
        tplContent = tplContent.Replace("{$FormControls}", formControls);
        return tplContent;
    }

    public string GenerateEditForm(List<PropertyInfo> propertyInfos, string name)
    {
        if (propertyInfos == null) return default;
        var formControls = "";
        foreach (var input in propertyInfos)
        {
            formControls += input.ToNgInputControl();
        }
        var tplContent = GetTplContent("NgViews.edit.edit.component.html.tpl");
        tplContent = tplContent.Replace("{$FormControls}", formControls);
        return tplContent;
    }

}
