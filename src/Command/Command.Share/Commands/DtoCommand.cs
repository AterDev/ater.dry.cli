using Definition.EntityFramework.DBProvider;

namespace Command.Share.Commands;

public class DtoCommand : CommandBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; set; }
    /// <summary>
    /// dto项目目录
    /// </summary>
    public string OutputPath { get; set; }

    /// <summary>
    /// 对应模块名
    /// </summary>
    public string? ModuleName { get; private set; }

    public DtoCodeGenerate CodeGen { get; set; }

    public DtoCommand(string entityPath, string outputPath)
    {
        EntityPath = entityPath;
        OutputPath = outputPath;

        CodeGen = new DtoCodeGenerate(EntityPath, OutputPath, new ContextBase());
        string entityName = Path.GetFileNameWithoutExtension(entityPath);
        Instructions.Add($"  🔹 generate {entityName} dtos.");
    }

    public async Task RunAsync(bool cover = false)
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine("🛑 Entity not exist!");
            return;
        }
        if (!Directory.Exists(OutputPath))
        {
            Console.WriteLine("🛑 Dto project not exist!");
            return;
        }
        if (CodeGen.EntityInfo == null)
        {
            Console.WriteLine("🛑 Entity parse failed!");
        }
        else
        {
            // 是否为模块
            var compilation = new CompilationHelper(OutputPath, "Entity");
            var content = File.ReadAllText(EntityPath);
            compilation.LoadContent(content);
            var attributes = compilation.GetClassAttribution("Module");
            if (attributes != null && attributes.Count != 0)
            {
                var argument = attributes.First().ArgumentList!.Arguments[0];
                ModuleName = compilation.GetArgumentValue(argument);
            }

            if (!string.IsNullOrWhiteSpace(ModuleName))
            {
                OutputPath = Path.Combine(Config.SolutionPath, "src", "Modules", ModuleName);
                CodeGen.AssemblyName = ModuleName;
            }
            if (Config.IsMicroservice)
            {
                CodeGen.AssemblyName = Config.ServiceName + ".Definition.Share";
            }
            Console.WriteLine(Instructions[0]);
            await SaveToFileAsync("Item", CodeGen.GetItemDto(), cover);
            await SaveToFileAsync("Short", CodeGen.GetShortDto(), cover);
            await SaveToFileAsync("Filter", CodeGen.GetFilterDto(), cover);
            await SaveToFileAsync("Add", CodeGen.GetAddDto(), cover);
            await SaveToFileAsync("Update", CodeGen.GetUpdateDto(), cover);

            if (string.IsNullOrWhiteSpace(ModuleName) && !Config.IsMicroservice)
            {
                await GenerateCommonFiles();
            }
            Console.WriteLine("😀 Dto generate completed!" + Environment.NewLine);

        }
    }
    public async Task GenerateCommonFiles()
    {
        List<string> globalUsings = CodeGen!.GetGlobalUsings();
        string filePath = Path.Combine(OutputPath, "GlobalUsings.cs");
        // 如果不存在则生成，如果存在，则添加
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            var newUsings = globalUsings.Where(g => !content.Contains(g))
                .ToList();
            if (newUsings.Count != 0)
            {
                newUsings.Insert(0, Environment.NewLine);
                File.AppendAllLines(filePath, newUsings);
            }
        }
        else
        {
            await GenerateFileAsync(OutputPath, "GlobalUsings.cs", string.Join(Environment.NewLine, globalUsings));
        }
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="dtoType"></param>
    /// <param name="content"></param>
    /// <param name="cover">是否覆盖</param>
    public async Task SaveToFileAsync(string dtoType, string? content, bool cover = false)
    {
        // 以文件名为准
        string entityName = Path.GetFileNameWithoutExtension(new FileInfo(EntityPath).Name);
        string outputDir = Path.Combine(OutputPath, "Models", entityName + "Dtos");
        await GenerateFileAsync(outputDir, $"{entityName}{dtoType}Dto.cs", content ?? "", cover);
    }
}
