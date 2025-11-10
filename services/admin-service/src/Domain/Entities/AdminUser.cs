namespace AdminService.Domain.Entities;

/// <summary>
/// Represents an Admin User in the system
/// </summary>
public class AdminUser
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public string Email { get; private set; } = string.Empty;
    
    public string FullName { get; private set; } = string.Empty;
    
    /// <summary>
    /// Status: active, suspended, banned
    /// </summary>
    public string Status { get; private set; } = "active";
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; private set; }
    
    public DateTime? SuspendedAt { get; private set; }
    
    public string? SuspensionReason { get; private set; }
    
    public int ProjectCount { get; private set; } = 0;
    
    public decimal TotalSpent { get; private set; } = 0;
    
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
