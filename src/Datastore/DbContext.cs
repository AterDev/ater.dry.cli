using LiteDB;

namespace Datastore;
public class DbContext : IDisposable
{
    public LiteDatabase Db { get; init; }
    public ILiteCollection<Project> Projects { get; set; }
    public ILiteCollection<EntityInfo> EntityInfos { get; set; }

    public DbContext()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localDir = Path.Combine(path, "AterStudio");
        var connectionString = $"Filename={Path.Combine(localDir, "droplet.db")};Upgrade=true;Connection=shared;initialSize=5MB";
        Db = new LiteDatabase(connectionString);

        Projects = Db.GetCollection<Project>();
        EntityInfos = Db.GetCollection<EntityInfo>();
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}
