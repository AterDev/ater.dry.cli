using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CodeGenerator.Test.Entity;

public class CommentEntityConfiguration : IEntityTypeConfiguration<Comments>
{
    public void Configure(EntityTypeBuilder<Comments> builder)
    {
        _ = builder.Property(b => b.Content)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
