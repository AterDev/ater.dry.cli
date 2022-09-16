using CodeGenerator.Infrastructure;

namespace Droplet.CommandLine.Commands;

/// <summary>
/// 控制器代码生成
/// </summary>
public class ApiCommand : CommandBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; }
    public string DtoPath { get; set; }
    /// <summary>
    /// service项目路径
    /// </summary>
    public string StorePath { get; }
    /// <summary>
    /// Http API路径
    /// </summary> 
    public string ApiPath { get; }

    public string Suffix { get; set; }
    public RestApiGenerate CodeGen { get; set; }

    public ApiCommand(string entityPath, string dtoPath, string servicePath, string apiPath, string? suffix = null)
    {
        EntityPath = entityPath;
        DtoPath = dtoPath;
        StorePath = servicePath;
        ApiPath = apiPath;
        Suffix = suffix ?? "Controller";
        CodeGen = new RestApiGenerate(entityPath, dtoPath, servicePath, apiPath, Suffix);
        var entityName = Path.GetFileNameWithoutExtension(entityPath);
        Instructions.Add("  🔹 generate interface & base class.");
        Instructions.Add($"  🔹 generate {entityName} RestApi.");
        Instructions.Add($"  🔹 update Globalusings files.");
    }
    public async Task RunAsync()
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine($"the {EntityPath} not exist");
            return;
        }
        Console.WriteLine(Instructions[0]);
        await GenerateCommonFilesAsync();
        Console.WriteLine(Instructions[1]);
        await GenerateRestApiAsync();
        Console.WriteLine(Instructions[2]);
        await GenerateGlobalUsingsFilesAsync();
        Console.WriteLine("😀 RestApi generate completed!" + Environment.NewLine);
    }

    private async Task GenerateGlobalUsingsFilesAsync()
    {
        var globalUsings = CodeGen.GetGlobalUsings();
        var filePath = Path.Combine(ApiPath, "GlobalUsings.cs");
        // 如果不存在则生成，如果存在，则添加
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);
            globalUsings = globalUsings.Where(g => !content.Contains(g))
                .ToList();

            if (globalUsings.Any())
            {
                globalUsings.Insert(0, Environment.NewLine);
                File.AppendAllLines(filePath, globalUsings);
            }
        }
        else
        {
            await GenerateFileAsync(ApiPath, "GlobalUsings.cs",
                string.Join(Environment.NewLine, globalUsings));
        }
    }

    private async Task GenerateRestApiAsync()
    {
        var apiDir = Path.Combine(ApiPath, "Controllers");
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        var apiContent = CodeGen.GetRestApiContent();
        await GenerateFileAsync(apiDir, $"{entityName}{Suffix}.cs", apiContent);
    }

    private async Task GenerateCommonFilesAsync()
    {
        var infrastructureDir = Path.Combine(ApiPath, "Infrastructure");
        var interfaceContent = CodeGen.GetRestApiInterface();
        var apiBaseContent = CodeGen.GetRestApiBase();
        await GenerateFileAsync(infrastructureDir, GenConst.IRESTAPI_BASE_NAME, interfaceContent);
        await GenerateFileAsync(infrastructureDir, GenConst.RESTAPI_BASE_NAME, apiBaseContent);
    }
}
