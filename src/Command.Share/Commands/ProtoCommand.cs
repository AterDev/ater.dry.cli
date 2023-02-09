using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command.Share.Commands;
public class ProtoCommand : CommandBase
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; set; }

    /// <summary>
    /// 项目路径
    /// </summary>
    public string ProjectPath { get; set; }

    public ProtobufGenerate CodeGen { get; set; }

    public ProtoCommand(string entityPath, string projectPath)
    {
        EntityPath = entityPath;
        ProjectPath = projectPath;
        CodeGen = new ProtobufGenerate(entityPath);
    }

    public async void Run(bool cover = false)
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine("🛑 Entity not exist!");
            return;
        }
        if (!Directory.Exists(ProjectPath))
        {
            Console.WriteLine("🛑 Entity not exist!");
            return;
        }
        if (CodeGen.EntityInfo == null)
        {
            Console.WriteLine("🛑 Entity parse failed!");
        }
        else
        {
            Console.WriteLine("generate protobuf to {0} from {1}",
                Path.GetDirectoryName(ProjectPath),
                Path.GetFileName(EntityPath));

            var content = CodeGen.GenerateProtobuf();

            await SaveToFileAsync(content, cover);
            Console.WriteLine("😀 Protobuf generate completed!" + Environment.NewLine);
        }
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="content"></param>
    /// <param name="cover">是否覆盖</param>
    public async Task SaveToFileAsync(string? content, bool cover = false)
    {
        // 以文件名为准
        string fileName = Path.GetFileNameWithoutExtension(EntityPath).ToHyphen('_');
        string outputDir = Path.Combine(ProjectPath, "Protos");
        await GenerateFileAsync(outputDir, $"{fileName}.proto", content ?? "", cover);
    }

}
