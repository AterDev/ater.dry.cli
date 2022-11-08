using Microsoft.EntityFrameworkCore;
using Studio.Entity;

namespace Studio;

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
