using Microsoft.EntityFrameworkCore;
using AdminService.Models;

namespace AdminService.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Template> Templates { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
    public DbSet<AdminUser> AdminUsers { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<DailyStatistic> DailyStatistics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AuditLog indexes
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.UserId);
        
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.CreatedAt);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.ResourceType);

        // Template indexes
        modelBuilder.Entity<Template>()
            .HasIndex(t => t.Category);

        modelBuilder.Entity<Template>()
            .HasIndex(t => t.IsActive);

        // SystemSetting indexes
        modelBuilder.Entity<SystemSetting>()
            .HasIndex(s => s.Key)
            .IsUnique();

        // AdminUser indexes
        modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Status);

        // Role indexes
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // UserRole indexes
        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => ur.RoleId);

        // DailyStatistic indexes
        modelBuilder.Entity<DailyStatistic>()
            .HasIndex(d => d.Date)
            .IsUnique();

        // Seed default roles
        var adminRoleId = Guid.Parse("12345678-1234-1234-1234-123456789001");
        var creatorRoleId = Guid.Parse("12345678-1234-1234-1234-123456789002");
        var viewerRoleId = Guid.Parse("12345678-1234-1234-1234-123456789003");

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = adminRoleId,
                Name = "Admin",
                Description = "Full system access",
                IsSystem = true,
                Permissions = new List<string> { "manage_users", "manage_roles", "view_analytics", "manage_templates", "manage_settings" }
            },
            new Role
            {
                Id = creatorRoleId,
                Name = "Creator",
                Description = "Can create and manage own projects",
                IsSystem = true,
                Permissions = new List<string> { "create_project", "view_own_projects", "generate_website" }
            },
            new Role
            {
                Id = viewerRoleId,
                Name = "Viewer",
                Description = "Read-only access",
                IsSystem = true,
                Permissions = new List<string> { "view_analytics_summary" }
            }
        );
    }
}
