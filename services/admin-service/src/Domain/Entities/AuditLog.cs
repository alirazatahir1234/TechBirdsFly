namespace AdminService.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking admin actions
/// </summary>
public class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public Guid? AdminUserId { get; private set; }
    
    /// <summary>
    /// The action performed (e.g., Create, Update, Delete, Suspend)
    /// </summary>
    public string Action { get; private set; } = string.Empty;
    
    /// <summary>
    /// Type of resource affected (e.g., User, Role, Setting)
    /// </summary>
    public string ResourceType { get; private set; } = string.Empty;
    
    /// <summary>
    /// ID of the affected resource
    /// </summary>
    public string ResourceId { get; private set; } = string.Empty;
    
    /// <summary>
    /// Additional details about the action
    /// </summary>
    public string? Details { get; private set; }
    
    /// <summary>
    /// JSON snapshot of old values
    /// </summary>
    public string? OldValues { get; private set; }
    
    /// <summary>
    /// JSON snapshot of new values
    /// </summary>
    public string? NewValues { get; private set; }
    
    public string IpAddress { get; private set; } = string.Empty;
    
    public string? UserAgent { get; private set; }
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual AdminUser? AdminUser { get; set; }

    // Domain Methods
    
    /// <summary>
    /// Creates a new audit log entry
    /// </summary>
    public static AuditLog Create(
        Guid? adminUserId,
        string action,
        string resourceType,
        string resourceId,
        string ipAddress,
        string? userAgent = null,
        string? details = null,
        string? oldValues = null,
        string? newValues = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));
        
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentException("ResourceType cannot be empty", nameof(resourceType));
        
        if (string.IsNullOrWhiteSpace(resourceId))
            throw new ArgumentException("ResourceId cannot be empty", nameof(resourceId));
        
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IpAddress cannot be empty", nameof(ipAddress));
        
        return new AuditLog
        {
            AdminUserId = adminUserId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Details = details,
            OldValues = oldValues,
            NewValues = newValues,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsModificationAction => 
        Action is "Create" or "Update" or "Delete" or "Modify" or "Suspend" or "Unsuspend" or "Ban";

    public bool IsReadonlyAction =>
        Action is "Read" or "Login" or "View";
}
