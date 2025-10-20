using Microsoft.EntityFrameworkCore;
using ImageService.Models;

namespace ImageService.Data;

public class ImageDbContext : DbContext
{
    public ImageDbContext(DbContextOptions<ImageDbContext> options) : base(options)
    {
    }

    public DbSet<Image> Images => Set<Image>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Image entity
        modelBuilder.Entity<Image>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<Image>()
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Image>()
            .Property(i => i.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Create indexes
        modelBuilder.Entity<Image>()
            .HasIndex(i => i.UserId);

        modelBuilder.Entity<Image>()
            .HasIndex(i => i.CreatedAt);

        modelBuilder.Entity<Image>()
            .HasIndex(i => i.Source);

        modelBuilder.Entity<Image>()
            .HasIndex(i => new { i.UserId, i.IsDeleted });
    }
}
