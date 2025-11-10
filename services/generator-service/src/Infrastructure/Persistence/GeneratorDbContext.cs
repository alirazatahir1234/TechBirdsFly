namespace GeneratorService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using GeneratorService.Domain.Entities;

/// <summary>
/// Entity Framework Core DbContext for Generator Service
/// SQLite database context with three main aggregates: Template, Project, Generation
/// </summary>
public class GeneratorDbContext : DbContext
{
    public GeneratorDbContext(DbContextOptions<GeneratorDbContext> options)
        : base(options)
    {
    }

    // Main DbSets
    public DbSet<Template> Templates { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Generation> Generations { get; set; } = null!;

    /// <summary>
    /// Configures the entity model with relationships, indices, and constraints
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Template Configuration
        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Content)
                .IsRequired();

            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.UseCount)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indices for filtering and searching
            entity.HasIndex(e => e.Type)
                .HasDatabaseName("IX_Template_Type");

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_Template_Category");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Template_IsActive");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Template_CreatedAt");

            entity.ToTable("Templates");
        });

        // Project Configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.TemplateId)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.OutputUrl)
                .HasMaxLength(500);

            entity.Property(e => e.Configuration)
                .IsRequired()
                .HasDefaultValue("{}");

            entity.Property(e => e.GenerationCount)
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.PublishedAt);

            // Foreign key relationship with Template - explicit configuration
            entity.HasOne<Template>()
                .WithMany()
                .HasForeignKey("TemplateId")
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Project_Template");

            // Indices for filtering and searching
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_Project_UserId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Project_Status");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Project_CreatedAt");

            entity.HasIndex(e => new { e.UserId, e.Status })
                .HasDatabaseName("IX_Project_UserId_Status");

            entity.ToTable("Projects");
        });

        // Generation Configuration
        modelBuilder.Entity<Generation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.ProjectId)
                .IsRequired();

            entity.Property(e => e.TemplateId)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(e => e.OutputPath)
                .HasMaxLength(500);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            entity.Property(e => e.Configuration)
                .IsRequired()
                .HasDefaultValue("{}");

            entity.Property(e => e.EstimatedCreditsUsed)
                .HasPrecision(10, 2);

            entity.Property(e => e.StartedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CompletedAt);

            // Foreign key relationships - explicit shadow property names to avoid conflicts
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Generation_Project");

            entity.HasOne<Template>()
                .WithMany()
                .HasForeignKey("TemplateId")
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Generation_Template");

            // Indices for filtering and searching
            entity.HasIndex(e => e.ProjectId)
                .HasDatabaseName("IX_Generation_ProjectId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Generation_Status");

            entity.HasIndex(e => e.StartedAt)
                .HasDatabaseName("IX_Generation_StartedAt");

            entity.HasIndex(e => new { e.ProjectId, e.Status })
                .HasDatabaseName("IX_Generation_ProjectId_Status");

            entity.ToTable("Generations");
        });
    }
}
