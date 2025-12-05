namespace AdminService.Domain.Entities;

/// <summary>
/// Represents an Admin User in the system
/// </summary>
public class AdminUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Status: active, suspended, banned
    /// </summary>
    public string Status { get; set; } = "active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? SuspendedAt { get; set; }

    public string? SuspensionReason { get; set; }

    public int ProjectCount { get; set; } = 0;

    public decimal TotalSpent { get; set; } = 0;

    // Navigation properties
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    // Domain Methods (Business Logic)

    /// <summary>
    /// Creates a new admin user
    /// </summary>
    public static AdminUser Create(string email, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));

        return new AdminUser
        {
            Email = email.ToLowerInvariant(),
            FullName = fullName,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Suspend(string reason)
    {
        if (Status == "active")
        {
            Status = "suspended";
            SuspendedAt = DateTime.UtcNow;
            SuspensionReason = reason;
        }
    }

    public void Unsuspend()
    {
        if (Status == "suspended")
        {
            Status = "active";
            SuspendedAt = null;
            SuspensionReason = null;
        }
    }

    public void Ban()
    {
        Status = "banned";
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdateProjectCount(int count)
    {
        ProjectCount = count;
    }

    public void UpdateTotalSpent(decimal amount)
    {
        TotalSpent = amount;
    }

    public bool IsActive => Status == "active";

    public bool IsSuspended => Status == "suspended";

    public bool IsBanned => Status == "banned";
}
