using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace AterStudio;

public class SwaggerFilter
{
}

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            OpenApiArray name = [];
            OpenApiArray enumData = [];
            System.Reflection.FieldInfo[] fields = context.Type.GetFields();
            foreach (System.Reflection.FieldInfo f in fields)
            {
                if (f.Name != "value__")
                {
                    name.Add(new OpenApiString(f.Name));
                    System.Reflection.CustomAttributeData? desAttr = f.CustomAttributes.Where(a => a.AttributeType.Name == "DescriptionAttribute").FirstOrDefault();

                    desAttr ??= f.CustomAttributes.Where(a => a.AttributeType.Name == "DisplayAttribute").FirstOrDefault();

                    if (desAttr != null)
                    {
                        System.Reflection.CustomAttributeTypedArgument des = desAttr.ConstructorArguments.FirstOrDefault();
                        if (des.Value != null)
                        {
                            enumData.Add(new OpenApiObject()
                            {
                                ["name"] = new OpenApiString(f.Name),
                                ["value"] = new OpenApiInteger((int)f.GetRawConstantValue()!),
                                ["description"] = new OpenApiString(des.Value.ToString())
                            });
                        }
                    }
                }
            }
            model.Extensions.Add("x-enumNames", name);
            model.Extensions.Add("x-enumData", enumData);
        }
    }
}
