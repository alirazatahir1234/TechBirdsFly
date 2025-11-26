using Microsoft.EntityFrameworkCore;
using GeneratorService.Domain.Entities;

namespace GeneratorService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for Generator Service
/// To be fully implemented in Phase 4
/// </summary>
public class GeneratorDbContext : DbContext
{
    public GeneratorDbContext(DbContextOptions<GeneratorDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Section> Sections { get; set; } = null!;
    public DbSet<GeneratedPage> GeneratedPages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeneratorDbContext).Assembly);
    }
}
