using Core.Entities;

using LiteDB;

namespace Datastore;
public class DbContext : IDisposable
{
    private static LiteDatabase? LiteDb { get; set; }
    public ILiteCollection<Project> Projects { get; set; }
    public ILiteCollection<EntityInfo> EntityInfos { get; set; }
    public ILiteCollection<ApiDocInfo> ApiDocInfos { get; set; }
    public ILiteCollection<TemplateFile> TemplateFile { get; set; }

    public DbContext()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localDir = Path.Combine(path, "AterStudio");
        var connectionString = $"Filename={Path.Combine(localDir, "dry.db")};Upgrade=true;initialSize=5MB";

        LiteDb ??= new LiteDatabase(connectionString);
        LiteDb.Mapper.EmptyStringToNull = false;
        Projects = LiteDb.GetCollection<Project>();
        EntityInfos = LiteDb.GetCollection<EntityInfo>();
        ApiDocInfos = LiteDb.GetCollection<ApiDocInfo>();
        TemplateFile = LiteDb.GetCollection<TemplateFile>();
    }
    ~DbContext() => Dispose();

    public void Dispose()
    {
        LiteDb?.Dispose();
    }
}
