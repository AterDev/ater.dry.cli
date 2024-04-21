using System.Linq.Expressions;


using Definition.Infrastructure.Helper;

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
            optionsBuilder.UseSqlite($"DataSource={path}");
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        OnSQLiteModelCreating(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }


    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entityEntry in entries)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property = entityEntry.Metadata.FindProperty("CreatedTime");
            if (property != null && property.ClrType == typeof(DateTimeOffset))
            {
                entityEntry.Property("CreatedTime").CurrentValue = DateTimeOffset.UtcNow;
            }
        }
        entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entityEntry in entries)
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty? property = entityEntry.Metadata.FindProperty("UpdatedTime");
            if (property != null && property.ClrType == typeof(DateTimeOffset))
            {
                entityEntry.Property("UpdatedTime").CurrentValue = DateTimeOffset.UtcNow;

            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    private void OnModelExtendCreating(ModelBuilder modelBuilder)
    {
        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes = modelBuilder.Model.GetEntityTypes();
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in entityTypes)
        {
            if (typeof(IEntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.Name).HasKey("Id");
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression<IEntityBase>(e => !e.IsDeleted, entityType.ClrType));
            }
        }
    }

    private static LambdaExpression ConvertFilterExpression<TInterface>(Expression<Func<TInterface, bool>> filterExpression, Type entityType)
    {
        ParameterExpression newParam = Expression.Parameter(entityType);
        Expression newBody = ReplacingExpressionVisitor.Replace(filterExpression.Parameters.Single(), newParam, filterExpression.Body);

        return Expression.Lambda(newBody, newParam);
    }

    /// <summary>
    /// sqlite的兼容处理
    /// </summary>
    /// <param name="modelBuilder"></param>
    private void OnSQLiteModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToStringConverter());
                }
            }
        }
    }

}
