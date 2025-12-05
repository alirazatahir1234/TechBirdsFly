using Microsoft.EntityFrameworkCore;
using PublishService.Domain.Entities;

namespace PublishService.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for PublishService
/// </summary>
public class PublishDbContext : DbContext
{
    public PublishDbContext(DbContextOptions<PublishDbContext> options) : base(options) { }

    public DbSet<PublishRecord> PublishRecords => Set<PublishRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PublishRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProjectId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.ProjectId, e.CreatedAt });
        });
    }
}
