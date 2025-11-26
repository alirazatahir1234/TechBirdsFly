using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeneratorService.Domain.Entities;
using GeneratorService.Domain.ValueObjects;

namespace GeneratorService.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for Section entity
/// Maps website section to database schema
/// </summary>
public class SectionConfig : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.HasKey(s => s.Id);

        // Scalar properties
        builder.Property(s => s.ProjectId)
            .IsRequired();

        builder.Property(s => s.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.CssClass)
            .HasMaxLength(500);

        // Audit properties
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt);

        // Owned value object: HtmlContent
        builder.OwnsOne(s => s.Html, html =>
        {
            html.Property(h => h.Value)
                .HasColumnName("Html")
                .IsRequired()
                .HasColumnType("text");
        });

        // Index for efficient queries
        builder.HasIndex(s => s.ProjectId)
            .HasDatabaseName("ix_sections_projectid");

        builder.HasIndex(s => s.Type)
            .HasDatabaseName("ix_sections_type");

        // Table configuration
        builder.ToTable("sections", schema: "public");
    }
}
