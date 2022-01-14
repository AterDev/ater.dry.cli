
namespace Droplet.CommandLine.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class StoreCommand : CommandBase
{
    public string EntityPath { get; set; }
    public string StorePath { get; set; }
    public string DtoPath { get; set; }
    public DataStoreGenerate CodeGen { get; set; }

    public StoreCommand(string entityPath, string servicePath)
    {
        EntityPath = entityPath;
        StorePath = servicePath;
        DtoPath = Config.SHARE_PATH;
        CodeGen = new DataStoreGenerate(entityPath, servicePath);
    }

    /// <summary>
    /// 生成仓储
    /// </summary>
    public void Run()
    {
        // 获取生成需要的实体名称
        if (!File.Exists(EntityPath))
        {
            return;
        }
        Console.WriteLine("仓储生成完成");
    }



    /// <summary>
    /// 生成常规文件
    /// </summary>
    public void GenerateCommonFiles()
    {

    }


}
