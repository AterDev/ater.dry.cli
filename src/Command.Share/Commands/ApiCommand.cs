using Core.Infrastructure;

namespace Command.Share.Commands;

/// <summary>
/// 控制器代码生成
/// </summary>
public class ApiCommand : CommandBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityFilePath { get; }
    public string DtoPath { get; private set; }
    /// <summary>
    /// service项目路径
    /// </summary>
    public string ApplicationPath { get; private set; }
    /// <summary>
    /// Http API路径
    /// </summary> 
    public string ApiPath { get; private set; }
    public string SolutionPath { get; }

    public string Suffix { get; set; }
    /// <summary>
    /// 对应模块名
    /// </summary>
    public string? ModuleName { get; private set; }
    public RestApiGenerate? CodeGen { get; set; }

    public ApiCommand(string entityPath, string dtoPath, string applicationPath, string apiPath, string? suffix = null)
    {
        EntityFilePath = entityPath;
        DtoPath = dtoPath;
        ApplicationPath = applicationPath;
        ApiPath = apiPath;

        var currentDir = new DirectoryInfo(apiPath);
        var solutionFile = AssemblyHelper.GetSlnFile(currentDir, currentDir.Root)
            ?? throw new Exception("not found solution file");

        SolutionPath = solutionFile.DirectoryName!;

        Suffix = suffix ?? "Controller";
        string entityName = Path.GetFileNameWithoutExtension(entityPath);
        Instructions.Add("  🔹 generate interface & base class.");
        Instructions.Add($"  🔹 generate {entityName} RestApi.");
        Instructions.Add($"  🔹 update Globalusings files.");
    }
    public async Task RunAsync(bool force = false)
    {
        if (!File.Exists(EntityFilePath))
        {
            Console.WriteLine($"the {EntityFilePath} not exist");
            return;
        }

        // 是否为模块
        var compilation = new CompilationHelper(ApplicationPath, "Entity");
        var content = File.ReadAllText(EntityFilePath);
        compilation.AddSyntaxTree(content);
        var attributes = compilation.GetClassAttribution("Module");
        if (attributes != null && attributes.Count != 0)
        {
            var argument = attributes.First().ArgumentList!.Arguments[0];
            ModuleName = compilation.GetArgumentValue(argument);
        }
        if (!string.IsNullOrWhiteSpace(ModuleName))
        {
            ApiPath = Path.Combine(SolutionPath, "src", "Modules", ModuleName);
            DtoPath = ApiPath;
            ApplicationPath = ApiPath;
        }

        CodeGen = new RestApiGenerate(EntityFilePath, DtoPath, ApplicationPath, ApiPath, Suffix);

        Console.WriteLine(Instructions[0]);
        await GenerateCommonFilesAsync();

        Console.WriteLine(Instructions[1]);
        await GenerateRestApiAsync(force);
        Console.WriteLine(Instructions[2]);
        await GenerateGlobalUsingsFilesAsync();


        Console.WriteLine("😀 RestApi generate completed!" + Environment.NewLine);
    }

    private async Task GenerateGlobalUsingsFilesAsync()
    {
        List<string> globalUsings = CodeGen!.GetGlobalUsings();
        string filePath = Path.Combine(ApiPath, "GlobalUsings.cs");
        // 如果不存在则生成，如果存在，则添加
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            globalUsings = globalUsings.Where(g => !content.Contains(g))
                .ToList();

            if (globalUsings.Count != 0)
            {
                File.AppendAllLines(filePath, globalUsings);
            }
        }
        else
        {
            await GenerateFileAsync(ApiPath, "GlobalUsings.cs",
                string.Join(Environment.NewLine, globalUsings));
        }
    }

    private async Task GenerateRestApiAsync(bool force)
    {
        string apiDir = Path.Combine(ApiPath, "Controllers");
        string adminDir = Path.Combine(ApiPath, "Controllers", "AdminControllers");
        if (!Directory.Exists(adminDir))
        {
            Directory.CreateDirectory(adminDir);
        }

        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        string apiContent = CodeGen!.GetRestApiContent();

        if (Config.IsSplitController == true)
        {
            string adminContent = apiContent
                .Replace(CodeGen.ApiNamespace + ".Controllers", CodeGen.ApiNamespace + ".Controllers.AdminControllers");
            await GenerateFileAsync(adminDir, $"{entityName}{Suffix}.cs", adminContent, force);
        }

        string clientContent = apiContent
            .Replace("RestControllerBase", "ClientControllerBase");

        await GenerateFileAsync(apiDir, $"{entityName}{Suffix}.cs", clientContent, force);
    }

    private async Task GenerateCommonFilesAsync()
    {
        string infrastructureDir = Path.Combine(ApiPath, "Infrastructure");
        string apiBaseContent = CodeGen!.GetRestApiBase();
        await GenerateFileAsync(infrastructureDir, GenConst.RESTAPI_BASE_NAME, apiBaseContent);
    }
}
