using Microsoft.OpenApi.Models;

namespace CodeGenerator.Generate;
public class DocGenerate : GenerateBase
{
    public IDictionary<string, OpenApiSchema> Schemas { get; set; }
    public List<OpenApiTag>? ApiTags { get; set; }
    public DocGenerate(IDictionary<string, OpenApiSchema> schemas)
    {
        Schemas = schemas;
    }
    public void SetTags(List<OpenApiTag> apiTags)
    {
        ApiTags = apiTags;
    }


    public string GetMarkdownContent()
    {
        var tsGen = new TSModelGenerate(Schemas);
        var content="";
        foreach (var schema in Schemas)
        {

            var header = $"### [{schema.Key}](#{schema.Key})" + Environment.NewLine;
            var props = tsGen.GetTsProperties(schema.Value);

            var row = "|字段名|类型|必须|说明|" + Environment.NewLine;
            row += "|-|-|-|-|" + Environment.NewLine;
            props.ForEach(p =>
            {
                row += FormatProperty(p);
            });
            content += header + row + Environment.NewLine;
        }
        return content;
    }

    public static string FormatProperty(TsProperty property)
    {

        var comments = property.Comments?.Replace("*","")
            .Replace("/","")
            .Replace("\r\n",Environment.NewLine)
            .Replace("\n\r",Environment.NewLine)
            .Replace("\r",Environment.NewLine)
            .Replace(Environment.NewLine+"*","")
            .Replace(Environment.NewLine,"")
            .Replace("\n",";")
            .Replace(" ","")
            .Trim(new char[] { ';' });

        if (string.IsNullOrEmpty(comments)) { comments = "-"; }

        var type = property.Type?.Replace(" | null","");
        var isMust = property.IsNullable==false?"false":"true";

        return $"|{property.Name}" +
            $"|{type}|{isMust}|{comments}|" +
            Environment.NewLine;
    }
}
