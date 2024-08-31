using RazorEngineCore;

/// <summary>
/// 代码生成下下文
/// </summary>
public class GenContext
{
    public IRazorEngine RazorEngine { get; set; } = new RazorEngine();

    public string GenManager(string templateContent, ManagerViewModel model)
    {
        return GenCode(templateContent, model);
    }

    public string GenCode<T>(string templateContent, T model)
    {
        IRazorEngineCompiledTemplate<RazorEngineTemplateBase<T>> template = RazorEngine.Compile<RazorEngineTemplateBase<T>>(templateContent);
        string result = template.Run(instance =>
        {
            instance.Model = model;
        });
        return result;
    }
}