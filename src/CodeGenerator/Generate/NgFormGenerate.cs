using PropertyInfo = Definition.Entity.PropertyInfo;

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
    public static string GenerateAddForm(List<PropertyInfo>? propertyInfos, bool isMobile = false)
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
        var tplDir = isMobile ? "mobileAdd" : "add";
        string tplContent = GetTplContent($"angular.{tplDir}.add.component.html.tpl");
        tplContent = tplContent.Replace(TplConst.FORM_CONTROLS, formControls);
        return tplContent;
    }

    /// <summary>
    /// 生成表单
    /// </summary>
    /// <param name="propertyInfos"></param>
    /// <returns></returns>
    public static string GenerateForm(List<PropertyInfo>? propertyInfos)
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

        string tplContent = GetTplContent("angular.component.form.component.html.tpl");
        tplContent = tplContent.Replace(TplConst.FORM_CONTROLS, formControls);
        return tplContent;
    }

    public static string GenerateEditForm(List<PropertyInfo>? propertyInfos, bool isMobile = false)
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
        var tplDir = isMobile ? "mobileEdit" : "edit";
        string tplContent = GetTplContent($"angular.{tplDir}.edit.component.html.tpl");
        tplContent = tplContent.Replace(TplConst.FORM_CONTROLS, formControls);
        return tplContent;
    }

}
