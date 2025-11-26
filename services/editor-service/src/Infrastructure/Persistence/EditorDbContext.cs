using Microsoft.EntityFrameworkCore;
using TechBirdsFly.EditorService.Domain.Entities;

namespace TechBirdsFly.EditorService.Infrastructure.Persistence;

public class EditorDbContext : DbContext
{
    public EditorDbContext(DbContextOptions<EditorDbContext> options)
        : base(options) { }

    public DbSet<Section> Sections => Set<Section>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Section>(e =>
        {
            e.HasKey(s => s.Id);

            e.Property(s => s.Id).ValueGeneratedNever();
            e.Property(s => s.ProjectId).IsRequired();
            e.Property(s => s.Type).IsRequired().HasMaxLength(100);
            e.Property(s => s.Html).IsRequired();
            e.Property(s => s.Css).HasMaxLength(5000);
            e.Property(s => s.Order).IsRequired();
            e.Property(s => s.CreatedAt).IsRequired();
            e.Property(s => s.UpdatedAt);

            e.HasIndex(s => s.ProjectId);
            e.HasIndex(s => new { s.ProjectId, s.Order });
        });
    }
}
