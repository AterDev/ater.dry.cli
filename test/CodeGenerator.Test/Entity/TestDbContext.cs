namespace CodeGenerator.Test.Entity;
public class TestDbContext : DbContext
{
    public DbSet<Comments> Comments { get; set; } = default!;
    public DbSet<Blog> Blogs { get; set; } = default!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("test");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(m =>
        {
            m.Property(nameof(Blog.Content))
                .HasMaxLength(5000);

        });
        base.OnModelCreating(modelBuilder);
    }
}
