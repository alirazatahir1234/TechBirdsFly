using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeneratorService.Domain.Entities;

namespace GeneratorService.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for GeneratedPage entity
/// Maps generated website page to database schema
/// </summary>
public class GeneratedPageConfig : IEntityTypeConfiguration<GeneratedPage>
{
    public void Configure(EntityTypeBuilder<GeneratedPage> builder)
    {
        builder.HasKey(p => p.Id);

        // Scalar properties
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(p => p.IsPublished)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit properties
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(p => p.UpdatedAt);

        // Owned value object: HtmlContent
        builder.OwnsOne(p => p.Html, html =>
        {
            html.Property(h => h.Value)
                .HasColumnName("Html")
                .IsRequired()
                .HasColumnType("text");
        });

        // Scalar columns for CSS and JavaScript
        builder.Property(p => p.Css)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.JavaScript)
            .HasColumnType("text")
            .IsRequired();

        // Owned value object: Metadata
        builder.OwnsOne(p => p.Meta, meta =>
        {
            meta.Property(m => m.Title)
                .HasColumnName("MetaTitle")
                .IsRequired()
                .HasMaxLength(255);

            meta.Property(m => m.Description)
                .HasColumnName("MetaDescription")
                .IsRequired()
                .HasMaxLength(500);

            meta.Property(m => m.Keywords)
                .HasColumnName("MetaKeywords")
                .HasMaxLength(500);
        });

        // Indexes for efficient queries
        builder.HasIndex(p => p.IsPublished)
            .HasDatabaseName("ix_generatedpages_ispublished");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("ix_generatedpages_createdat");

        // Table configuration
        builder.ToTable("generated_pages", schema: "public");
    }
}
