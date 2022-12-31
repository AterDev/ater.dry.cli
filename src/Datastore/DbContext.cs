using LiteDB;

namespace Datastore;
public class DbContext
{
    public LiteDatabase Db { get; init; }
    public ILiteCollection<Project> Projects { get; set; }
    public ILiteCollection<EntityInfo> EntityInfos { get; set; }
    public ILiteCollection<ApiDocInfo> ApiDocInfos { get; set; }

    public DbContext()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localDir = Path.Combine(path, "AterStudio");
        var connectionString = $"Filename={Path.Combine(localDir, "droplet.db")};Upgrade=true;Connection=shared;initialSize=5MB";
        Db = new LiteDatabase(connectionString);
        Db.Mapper.EmptyStringToNull = false;

        Projects = Db.GetCollection<Project>();
        EntityInfos = Db.GetCollection<EntityInfo>();
        ApiDocInfos = Db.GetCollection<ApiDocInfo>();
    }
    ~DbContext() => Db.Dispose();
}
