using PropertyInfo = Core.Models.PropertyInfo;

namespace CodeGenerator.Generate;

/// <summary>
/// 表单生成
/// </summary>
public class NgFormGenerate : GenerateBase
{
    public NgFormGenerate()
    {
    }

    /// <summary>
    /// 生成添加组件
    /// </summary>
    public static string GenerateAddForm(List<PropertyInfo>? propertyInfos)
    {
        string formControls = "";
        if (propertyInfos != null)
        {
            foreach (PropertyInfo input in propertyInfos)
            {
                NgInputBuilder inputBuilder = new(input.Type, input.Name, input.CommentSummary ?? input.DisplayName)
                {
                    IsDecimal = input.IsDecimal,
                    IsRequired = input.IsRequired,
                    MaxLength = input.MaxLength,
                    MinLength = input.MinLength,
                    IsEnum = input.IsEnum,
                    IsList = input.IsList
                };
                formControls += inputBuilder.ToFormControl();
            }
        }

        string tplContent = GetTplContent("angular.add.add.component.html.tpl");
        tplContent = tplContent.Replace("{$FormControls}", formControls);
        return tplContent;
    }

    public static string GenerateEditForm(List<PropertyInfo>? propertyInfos)
    {
        string formControls = "";
        if (propertyInfos != null)
        {
            foreach (PropertyInfo input in propertyInfos)
            {
                NgInputBuilder inputBuilder = new(input.Type, input.Name, input.DisplayName)
                {
                    IsDecimal = input.IsDecimal,
                    IsRequired = input.IsRequired,
                    MaxLength = input.MaxLength,
                    MinLength = input.MinLength,
                    IsEnum = input.IsEnum,
                    IsList = input.IsList
                };
                formControls += inputBuilder.ToFormControl();
            }
        }

        string tplContent = GetTplContent("angular.edit.edit.component.html.tpl");
        tplContent = tplContent.Replace("{$FormControls}", formControls);
        return tplContent;
    }

}
