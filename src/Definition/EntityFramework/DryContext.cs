using Definition.Infrastructure.Helper;

namespace Definition.EntityFramework;

public class DryContext : DbContext
{
    public DbSet<Project> Projects { get; set; }

    public DbSet<EntityInfo> EntityInfos { get; set; }

    public DbSet<ApiDocInfo> ApiDocInfos { get; set; }

    public DbSet<TemplateFile> TemplateFiles { get; set; }

    public DbSet<ConfigData> Configs { get; set; }


    public DryContext(DbContextOptions options) : base(options)
    {
    }
    public DryContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var path = Path.Combine(AssemblyHelper.GetStudioPath(), "ater.dry.db");
            optionsBuilder.UseSqlite($"Source={path}");
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

}
