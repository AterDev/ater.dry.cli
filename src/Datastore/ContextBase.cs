using Datastore.Entity;

namespace Datastore;

public class ContextBase : DbContext
{
    public DbSet<Project> Projects { get; set; }

    public DbSet<EntityInfo> EntityInfos { get; set; }

    public DbSet<PropertyInfo> PropertyInfos { get; set; }


    public ContextBase()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        //var connectionString = $"Data Source={Path.Combine(path, "AterStudio", "studio.db")}";

        string connectionString = $"Data Source={Path.Combine(path, "studio.db")}";
        _ = optionsBuilder.UseSqlite(connectionString);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
