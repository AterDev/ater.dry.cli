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

    public void Generate()
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine("Entity not exist!");
            return;
        }
        if (!Directory.Exists(DtoPath))
        {
            Console.WriteLine("DtoPath not exist!");
            return;
        }
        //var gen = new DtoGenerate();
    }


}
