namespace Command.Share.Commands;
public class ProtoCommand(string entityPath, string projectPath) : CommandBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; set; } = entityPath;
    /// <summary>
    /// 项目路径
    /// </summary>
    public string ProjectPath { get; set; } = projectPath;
    public ProtobufGenerate CodeGen { get; set; } = new ProtobufGenerate(entityPath);

    public string? ErrorMessage { get; set; }

    public async Task RunAsync(bool cover = false)
    {
        var projectName = Path.GetFileNameWithoutExtension(ProjectPath);
        var projectDir = Path.GetDirectoryName(ProjectPath);
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine("🛑 Entity not exist!");
            ErrorMessage = "🛑 Entity not exist!";
            return;
        }
        if (!Directory.Exists(projectDir))
        {
            Console.WriteLine("🛑 Project not exist!");
            ErrorMessage = "🛑 Project not exist!";
            return;
        }
        if (CodeGen.EntityInfo == null)
        {
            Console.WriteLine("🛑 Entity parse failed!");
            ErrorMessage = "🛑 Entity parse failed!";
        }
        else
        {
            Console.WriteLine("generate protobuf to {0} from {1}",
                projectName,
                Path.GetFileName(EntityPath));

            var content = CodeGen.GenerateProtobuf();

            await SaveToFileAsync(projectDir, content, cover);
            Console.WriteLine("😀 Protobuf generate completed!" + Environment.NewLine);
        }
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="content"></param>
    /// <param name="cover">是否覆盖</param>
    public async Task SaveToFileAsync(string path, string? content, bool cover = false)
    {
        // 以文件名为准
        string fileName = Path.GetFileNameWithoutExtension(EntityPath).ToHyphen('_');
        string outputDir = Path.Combine(path, "Protos");
        await GenerateFileAsync(outputDir, $"{fileName}.proto", content ?? "", cover);
    }
}
