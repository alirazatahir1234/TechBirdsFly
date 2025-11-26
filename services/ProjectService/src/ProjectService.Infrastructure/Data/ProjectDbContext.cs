using Microsoft.EntityFrameworkCore;
using ProjectService.Domain.Entities;

namespace ProjectService.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Project Service
/// Manages all project-related entities and database operations
/// </summary>
public class ProjectDbContext : DbContext
{
    /// <summary>
    /// Initialize DbContext with options
    /// </summary>
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Projects DbSet
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// ProjectVersions DbSet
    /// </summary>
    public DbSet<ProjectVersion> Versions => Set<ProjectVersion>();

    /// <summary>
    /// ProjectArtifacts DbSet
    /// </summary>
    public DbSet<ProjectArtifact> Artifacts => Set<ProjectArtifact>();

    /// <summary>
    /// Configure model relationships
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project -> Versions relationship (1 to many)
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Versions)
            .WithOne()
            .HasForeignKey(v => v.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectVersion -> ProjectArtifacts relationship (1 to many)
        modelBuilder.Entity<ProjectVersion>()
            .HasMany(v => v.Artifacts)
            .WithOne()
            .HasForeignKey(a => a.VersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create indexes for common queries
        modelBuilder.Entity<Project>()
            .HasIndex(p => p.OwnerId);

        modelBuilder.Entity<ProjectVersion>()
            .HasIndex(v => v.ProjectId);

        modelBuilder.Entity<ProjectArtifact>()
            .HasIndex(a => a.VersionId);

        modelBuilder.Entity<ProjectArtifact>()
            .HasIndex(a => a.ArtifactId);
    }
}
