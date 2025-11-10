using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechBirdsFly.AdminService.Domain.Entities;

namespace TechBirdsFly.AdminService.Infrastructure.Configurations;

/// <summary>
/// Entity Type Configuration for AdminUser.
/// Defines fluent API configuration for the AdminUser entity mapping.
/// This configuration is applied via AdminDbContext.OnModelCreating.
/// </summary>
public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        // Timestamps with server defaults
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasMany(e => e.Roles)
            .WithMany(r => r.AdminUsers)
            .UsingEntity("AdminUserRoles");

        builder.HasMany(e => e.AuditLogs)
            .WithOne(a => a.AdminUser)
            .HasForeignKey(a => a.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity Type Configuration for Role.
/// Defines fluent API configuration for the Role entity mapping.
/// This configuration is applied via AdminDbContext.OnModelCreating.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsSystem)
            .HasDefaultValue(false);

        // Timestamps with server defaults
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Permissions stored as JSON array
        builder.Property(e => e.Permissions)
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        // Indexes
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.IsSystem);
    }
}

/// <summary>
/// Entity Type Configuration for AuditLog.
/// Defines fluent API configuration for the AuditLog entity mapping.
/// This configuration is applied via AdminDbContext.OnModelCreating.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AdminUserId)
            .IsRequired();

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ResourceId)
            .HasMaxLength(256);

        // JSON fields
        builder.Property(e => e.Details)
            .HasColumnType("jsonb");

        builder.Property(e => e.OldValues)
            .HasColumnType("jsonb");

        builder.Property(e => e.NewValues)
            .HasColumnType("jsonb");

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        // Timestamp with server default
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for query performance
        builder.HasIndex(e => e.AdminUserId);
        builder.HasIndex(e => e.Action);
        builder.HasIndex(e => e.ResourceType);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.AdminUserId, e.CreatedAt });

        // Relationship
        builder.HasOne(e => e.AdminUser)
            .WithMany(a => a.AuditLogs)
            .HasForeignKey(e => e.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
