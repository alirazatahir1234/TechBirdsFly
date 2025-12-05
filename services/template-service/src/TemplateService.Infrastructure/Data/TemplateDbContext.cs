using Microsoft.EntityFrameworkCore;
using TemplateService.Domain.Entities;

namespace TemplateService.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Template Service
/// </summary>
public class TemplateDbContext : DbContext
{
    public TemplateDbContext(DbContextOptions<TemplateDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet for Templates
    /// </summary>
    public DbSet<Template> Templates => Set<Template>();

    /// <summary>
    /// DbSet for Template Files
    /// </summary>
    public DbSet<TemplateFile> TemplateFiles => Set<TemplateFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Template entity
        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PreviewImageUrl).HasMaxLength(2048);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure one-to-many relationship
            entity.HasMany(t => t.Files)
                .WithOne()
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TemplateFile entity
        modelBuilder.Entity<TemplateFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(2048);
            entity.Property(e => e.Format).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.TemplateId);
        });
    }
}
