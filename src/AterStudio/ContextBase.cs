using AterStudio.Entity;
using Microsoft.EntityFrameworkCore;

namespace AterStudio;

public class ContextBase : DbContext
{
    public DbSet<Project> Projects { get; set; }

    public ContextBase(DbContextOptions<ContextBase> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
