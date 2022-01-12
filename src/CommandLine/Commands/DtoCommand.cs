using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droplet.CommandLine.Commands;

public class DtoCommand
{
    /// <summary>
    /// 实体文件路径
    /// </summary>
    public string EntityPath { get; set; }
    /// <summary>
    /// dto项目目录
    /// </summary>
    public string DtoPath { get; set; }

    public DtoCommand(string entityPath, string dtoPath)
    {
        EntityPath = entityPath;
        DtoPath = dtoPath;
    }

    public void Generate(bool cover = false)
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine("Entity not exist!");
            return;
        }
        if (!Directory.Exists(DtoPath))
        {
            Console.WriteLine("Dto project not exist!");
            return;
        }
        var gen = new DtoGenerate(EntityPath)
        {
            AssemblyName = new DirectoryInfo(DtoPath).Name
        };
        if (gen.EntityInfo == null)
        {
            Console.WriteLine("Entity parse failed!");
        }
        else
        {
            SaveToFile("Update", gen.GetUpdateDto(), cover);
            SaveToFile("Filter", gen.GetFilterDto(), cover);
            SaveToFile("Item", gen.GetItemDto(), cover);
            SaveToFile("Short", gen.GetShortDto(), cover);

            GenerateUsingsFile(gen.GetDtoUsings());
            GenerateFilterBaseFile(gen.GetFilterBase());
        }
    }
    public void GenerateUsingsFile(string content)
    {
        var outputDir = Path.Combine(DtoPath, "Models", "GlobalUsing.cs");
        if (!File.Exists(outputDir))
            File.WriteAllTextAsync(outputDir, content);
    }
    public void GenerateFilterBaseFile(string content)
    {
        var outputDir = Path.Combine(DtoPath, "Models", "FilterBase.cs");
        if (!File.Exists(outputDir))
            File.WriteAllTextAsync(outputDir, content);
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="dtoType"></param>
    /// <param name="content"></param>
    /// <param name="cover">是否覆盖</param>
    public async void SaveToFile(string dtoType, string? content, bool cover = false)
    {
        // 以文件名为准
        var entityName = Path.GetFileNameWithoutExtension(new FileInfo(EntityPath).Name);
        var outputDir = Path.Combine(DtoPath, "Models", entityName + "Dtos");
        var filePath = Path.Combine(outputDir, $"{entityName}{dtoType}Dto.cs");
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        if (!File.Exists(filePath) || cover)
        {
            await File.WriteAllTextAsync(filePath, content);
        }
    }
}
