namespace AdminService.Domain.Events;

/// <summary>
/// Raised when an admin user is created
/// </summary>
public class AdminUserCreatedEvent
{
    public Guid AdminUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when an admin user is suspended
/// </summary>
public class AdminUserSuspendedEvent
{
    public Guid AdminUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime SuspendedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when an admin user is unsuspended
/// </summary>
public class AdminUserUnsuspendedEvent
{
    public Guid AdminUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime UnsuspendedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when an admin user is banned
/// </summary>
public class AdminUserBannedEvent
{
    public Guid AdminUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when an admin user logs in
/// </summary>
public class AdminUserLoginEvent
{
    public Guid AdminUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;
}
