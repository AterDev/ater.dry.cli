using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace CodeGenerator.Generate;
public class TestGenerate : GenerateBase
{
    protected OpenApiPaths PathsPairs { get; }
    protected List<OpenApiTag> ApiTags { get; }
    public IDictionary<string, OpenApiSchema> Schemas { get; set; }
    public OpenApiDocument OpenApi { get; set; }


    public TestGenerate(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        PathsPairs = openApi.Paths;
        Schemas = openApi.Components.Schemas;
        ApiTags = openApi.Tags.ToList();
    }



    public string GetFilterTestContent()
    {
        // 查询 路由以 filter 结尾的接口
        var filterPaths = PathsPairs.Where(p => p.Key.EndsWith("filter")).ToList();
        var content = "";
        foreach (var path in filterPaths)
        {
            var operation = path.Value.Operations.FirstOrDefault();
            content += GenerateFilterTestMethod(path.Key, operation.Value.OperationId.Replace("_", ""), operation.Key == OperationType.Get);
        }
        return content;
    }


    /// <summary>
    /// 生成请求方法
    /// </summary>
    /// <param name="url"></param>
    /// <param name="name">方法名</param>
    /// <param name="isGet">是否是get请求</param>
    /// <returns></returns>
    public string GenerateFilterTestMethod(string url, string name, bool isGet = false)
    {
        var request = isGet ? "GetAsync(url);" : "PostAsJsonAsync(url, data);";

        return $$"""
    [Theory]
    [InlineData("{{url}}")]
    public async Task Should_Filter{{name}}Async(string url)
    {
        var data = new {
            PageIndex = 1,
            PageSize = 2,
        };
        var response = await _client.{{request}}
        response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        Assert.NotNull(res);
    }
    """;
    }


    public string GenerateFilterMethod(string url, string name, OperationType type)
    {
        var dataString = "";
        var requestString = "GetAsync(url);";
        if (type == OperationType.Post || type == OperationType.Put)
        {
            dataString = "var data = new {};";
            requestString = "PostAsJsonAsync(url, data);";
        }

        return $$"""
    [Theory]
    [InlineData("{{url}}")]
    public async Task Should_EditAsync(string url)
    {
        // TODO: 定义参数对象
        {{dataString}}
        //url = string.Format(url, id);

        var response = await _client.{{requestString}}
        response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        Assert.NotNull(res);
    }
    """;

    }
}
