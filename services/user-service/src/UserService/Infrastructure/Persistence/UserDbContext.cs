using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User aggregate
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.Bio)
                .HasMaxLength(1000);

            // Own Email (value object)
            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("Email");

                email.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email_Unique");
            });

            // Own Phone (value object)
            entity.OwnsOne(e => e.Phone, phone =>
            {
                phone.Property(e => e.Value)
                    .HasMaxLength(20)
                    .HasColumnName("Phone");

                phone.HasIndex(e => e.Value)
                    .HasDatabaseName("IX_Users_Phone");
            });

            // Enum conversion
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.EmailVerified);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.LastLoginAt);
            entity.Property(e => e.LoginAttempts);
            entity.Property(e => e.LockoutUntil);

            // Indexes for performance
            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username_Unique");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Users_Status");

            entity.HasIndex(e => e.Role)
                .HasDatabaseName("IX_Users_Role");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Users_CreatedAt");

            entity.HasIndex(e => e.LastLoginAt)
                .HasDatabaseName("IX_Users_LastLoginAt");

            entity.HasIndex(e => e.LockoutUntil)
                .HasDatabaseName("IX_Users_LockoutUntil");

            // Relationships
            entity.HasMany<UserProfile>()
                .WithOne()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.CompanyName)
                .HasMaxLength(255);

            entity.Property(e => e.Department)
                .HasMaxLength(255);

            entity.Property(e => e.JobTitle)
                .HasMaxLength(255);

            entity.Property(e => e.Location)
                .HasMaxLength(255);

            entity.Property(e => e.Website)
                .HasMaxLength(500);

            entity.Property(e => e.Preferences)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);

            // Indexes
            entity.HasIndex(e => e.UserId)
                .IsUnique()
                .HasDatabaseName("IX_UserProfiles_UserId_Unique");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_UserProfiles_CreatedAt");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}

#region User Repository

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<User> Items, int Total)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "username" => ascending ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username),
                "email" => ascending ? query.OrderBy(u => u.Email.Value) : query.OrderByDescending(u => u.Email.Value),
                "created" => ascending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                "lastlogin" => ascending ? query.OrderBy(u => u.LastLoginAt) : query.OrderByDescending(u => u.LastLoginAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        var total = await query.CountAsync(cancellationToken);
        var skip = (pageNumber - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Role.ToString() == role)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Status.ToString() == status)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Users.AddAsync(user, cancellationToken);
        return entry.Entity;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        return await Task.FromResult(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

#endregion

#region User Profile Repository

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UserDbContext _context;

    public UserProfileRepository(UserDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<UserProfile> AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        var entry = await _context.UserProfiles.AddAsync(profile, cancellationToken);
        return entry.Entity;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        _context.UserProfiles.Update(profile);
        return await Task.FromResult(profile);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken);
        if (profile == null)
            return false;

        _context.UserProfiles.Remove(profile);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

#endregion
