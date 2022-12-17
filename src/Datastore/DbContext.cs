using LiteDB;

namespace Datastore;
public class DbContext : IDisposable
{
    public LiteDatabase Db { get; init; }
    public ILiteCollection<Project> Projects { get; set; }
    public ILiteCollection<EntityInfo> EntityInfos { get; set; }
    public ILiteCollection<PropertyInfo> PropertyInfos { get; set; }


    public DbContext()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localDir = Path.Combine(path, "AterStudio");
        Db = new LiteDatabase(Path.Combine(localDir, "studio.db"));

        Projects = Db.GetCollection<Project>();
        EntityInfos = Db.GetCollection<EntityInfo>();
        PropertyInfos = Db.GetCollection<PropertyInfo>();
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}
