using Datastore;

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
    public string DtoPath { get; set; }
    public DtoCodeGenerate CodeGen { get; set; }

    public DtoCommand(string entityPath, string dtoPath)
    {
        EntityPath = entityPath;
        DtoPath = dtoPath;

        CodeGen = new DtoCodeGenerate(EntityPath, DtoPath, CommandRunner.dbContext)
        {
            AssemblyName = new DirectoryInfo(DtoPath).Name
        };
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
        if (!Directory.Exists(DtoPath))
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
            Console.WriteLine(Instructions[0]);
            await SaveToFileAsync("Add", CodeGen.GetAddDto(), cover);
            await SaveToFileAsync("Update", CodeGen.GetUpdateDto(), cover);
            await SaveToFileAsync("Filter", CodeGen.GetFilterDto(), cover);
            await SaveToFileAsync("Item", CodeGen.GetItemDto(), cover);
            await SaveToFileAsync("Short", CodeGen.GetShortDto(), cover);
            GenerateCommonFiles();
            Console.WriteLine("😀 Dto generate completed!" + Environment.NewLine);
        }
    }
    public async void GenerateCommonFiles()
    {
        await GenerateFileAsync(DtoPath, "GlobalUsings.cs", CodeGen.GetDtoUsings());
        await GenerateFileAsync("FilterBase.cs", CodeGen.GetFilterBase());
        await GenerateFileAsync("PageList.cs", CodeGen.GetPageList());
        await GenerateFileAsync("BatchUpdate.cs", CodeGen.GetBatchUpdate());
        //await GenerateFileAsync("EntityBase.cs", CodeGen.GetEntityBase());
    }

    public async Task GenerateFileAsync(string fileName, string content)
    {
        await GenerateFileAsync(Path.Combine(DtoPath, "Models"), fileName, content);
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
        string outputDir = Path.Combine(DtoPath, "Models", entityName + "Dtos");
        await GenerateFileAsync(outputDir, $"{entityName}{dtoType}Dto.cs", content ?? "", cover);
    }
}
