using LiteDB;

namespace Datastore;
public class DbContext
{
    public static LiteDatabase? Db = null;
    public ILiteCollection<Project> Projects { get; set; }
    public ILiteCollection<EntityInfo> EntityInfos { get; set; }
    public ILiteCollection<ApiDocInfo> ApiDocInfos { get; set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public DbContext()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localDir = Path.Combine(path, "AterStudio");
        var connectionString = $"Filename={Path.Combine(localDir, "droplet.db")};Upgrade=true;initialSize=5MB";
        try
        {
            if (Db == null)
            {
                Db = new LiteDatabase(connectionString);
                Db.Mapper.EmptyStringToNull = false;
            }
            Projects = Db.GetCollection<Project>();
            EntityInfos = Db.GetCollection<EntityInfo>();
            ApiDocInfos = Db.GetCollection<ApiDocInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ",请尝试结束studio后再执行命令");
            return;
        }
    }
}
