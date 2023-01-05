using LiteDB;

namespace Datastore;
public class DbContext
{
    public static LiteDatabase? Db = null;
    public ILiteCollection<Project> Projects { get; set; }
    public ILiteCollection<EntityInfo> EntityInfos { get; set; }
    public ILiteCollection<ApiDocInfo> ApiDocInfos { get; set; }

    public DbContext()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localDir = Path.Combine(path, "AterStudio");
        var connectionString = $"Filename={Path.Combine(localDir, "droplet.db")};Upgrade=true;initialSize=5MB";

        if (Db == null)
        {
            try
            {
                Db = new LiteDatabase(connectionString);
                Db.Mapper.EmptyStringToNull = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "请尝试结束studio后再执行命令");
                return;
            }
        }
        Projects = Db.GetCollection<Project>();
        EntityInfos = Db.GetCollection<EntityInfo>();
        ApiDocInfos = Db.GetCollection<ApiDocInfo>();
    }
}
