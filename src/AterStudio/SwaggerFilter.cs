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
            //model.Enum.Clear();
            //model.Description = "desp";
            var name = new OpenApiArray();
            Enum.GetNames(context.Type)
                .ToList()
                .ForEach(n =>
                {
                    name.Add(new OpenApiString(n));
                });
            model.Extensions.Add("x-enumNames", name);
        }
    }
}