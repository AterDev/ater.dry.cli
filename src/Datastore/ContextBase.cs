namespace Datastore;

public class ContextBase : DbContext
{
    public DbSet<Project> Projects { get; set; }

    public DbSet<EntityInfo> EntityInfos { get; set; }

    public DbSet<PropertyInfo> PropertyInfos { get; set; }

    public ContextBase()
    {
    }

    public ContextBase(DbContextOptions<ContextBase> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var localDir = Path.Combine(path, "AterStudio");
        if (!Directory.Exists(localDir))
        {
            Directory.CreateDirectory(localDir);
        }

        var connectionString = $"Data Source={Path.Combine(localDir, "studio.db")}";
        //string connectionString = $"Data Source={Path.Combine(path, "studio.db")}";

        _ = optionsBuilder.UseSqlite(connectionString, a => a.MigrationsAssembly("Datastore"));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<EntityInfo>(e =>
        {
            e.HasIndex(p => p.Name);
            e.HasIndex(p => p.NamespaceName);
            e.HasIndex(p => p.ProjectId);
        });

        modelBuilder.Entity<PropertyInfo>(e =>
        {
            e.HasIndex(p => p.Name);
            e.HasIndex(p => p.Type);
            e.HasIndex(p => p.ProjectId);
        });
        base.OnModelCreating(modelBuilder);
    }
}
