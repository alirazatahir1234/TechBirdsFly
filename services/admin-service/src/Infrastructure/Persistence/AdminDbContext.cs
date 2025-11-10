using Microsoft.EntityFrameworkCore;
using TechBirdsFly.AdminService.Domain.Entities;

namespace TechBirdsFly.AdminService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for Admin Service.
/// Manages database schema, entities, and relationships for AdminUser, Role, and AuditLog entities.
/// </summary>
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AdminUser entity
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index for email lookups
            entity.HasIndex(e => e.Email)
                .IsUnique();

            // Index for status queries
            entity.HasIndex(e => e.Status);

            // Configure relationships
            entity.HasMany(e => e.Roles)
                .WithMany(r => r.AdminUsers)
                .UsingEntity("AdminUserRoles");

            entity.HasMany(e => e.AuditLogs)
                .WithOne(a => a.AdminUser)
                .HasForeignKey(a => a.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.IsSystem)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Permissions stored as JSON array
            entity.Property(e => e.Permissions)
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            // Index for name lookups
            entity.HasIndex(e => e.Name)
                .IsUnique();

            // Index for system roles
            entity.HasIndex(e => e.IsSystem);
        });

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.AdminUserId)
                .IsRequired();

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ResourceType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ResourceId)
                .HasMaxLength(256);

            entity.Property(e => e.Details)
                .HasColumnType("jsonb");

            entity.Property(e => e.OldValues)
                .HasColumnType("jsonb");

            entity.Property(e => e.NewValues)
                .HasColumnType("jsonb");

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes for common queries
            entity.HasIndex(e => e.AdminUserId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.ResourceType);
            entity.HasIndex(e => e.CreatedAt);

            // Composite index for user + date range queries
            entity.HasIndex(e => new { e.AdminUserId, e.CreatedAt });

            // Foreign key relationship
            entity.HasOne(e => e.AdminUser)
                .WithMany(a => a.AuditLogs)
                .HasForeignKey(e => e.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed system roles
        SeedSystemRoles(modelBuilder);
    }

    private static void SeedSystemRoles(ModelBuilder modelBuilder)
    {
        var superAdminRole = new Role
        {
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            Name = "SuperAdmin",
            Description = "Super Administrator with full system access",
            IsSystem = true,
            Permissions = new List<string>
            {
                "admin.users.view",
                "admin.users.create",
                "admin.users.update",
                "admin.users.delete",
                "admin.users.suspend",
                "admin.users.ban",
                "admin.roles.view",
                "admin.roles.create",
                "admin.roles.update",
                "admin.roles.delete",
                "admin.audit.view",
                "admin.system.configure"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var adminRole = new Role
        {
            Id = new Guid("00000000-0000-0000-0000-000000000002"),
            Name = "Admin",
            Description = "Administrator with limited system access",
            IsSystem = true,
            Permissions = new List<string>
            {
                "admin.users.view",
                "admin.users.create",
                "admin.users.update",
                "admin.users.suspend",
                "admin.roles.view",
                "admin.audit.view"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var moderatorRole = new Role
        {
            Id = new Guid("00000000-0000-0000-0000-000000000003"),
            Name = "Moderator",
            Description = "Moderator with limited moderation capabilities",
            IsSystem = true,
            Permissions = new List<string>
            {
                "admin.users.view",
                "admin.users.suspend",
                "admin.audit.view"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Role>().HasData(superAdminRole, adminRole, moderatorRole);
    }
}
