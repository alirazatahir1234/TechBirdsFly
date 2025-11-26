using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeneratorService.Domain.Entities;

namespace GeneratorService.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for Project entity
/// Maps aggregate root to database schema
/// </summary>
public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        // Scalar properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Industry)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Style)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        // Audit properties
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt);

        // Owned value object: ColorPalette
        builder.OwnsOne(p => p.Palette, palette =>
        {
            palette.Property(p => p.Primary)
                .HasColumnName("PrimaryColor")
                .IsRequired()
                .HasMaxLength(7);

            palette.Property(p => p.Secondary)
                .HasColumnName("SecondaryColor")
                .IsRequired()
                .HasMaxLength(7);

            palette.Property(p => p.Accent)
                .HasColumnName("AccentColor")
                .IsRequired()
                .HasMaxLength(7);
        });

        // Relationships
        builder.HasMany(typeof(Section))
            .WithOne()
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        // Table configuration
        builder.ToTable("projects", schema: "public");
    }
}
