namespace AdminService.Domain.Entities;

/// <summary>
/// Represents a Role that can be assigned to admin users
/// </summary>
public class Role
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Role name (e.g., Admin, Creator, Viewer)
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Comma-separated or JSON list of permissions
    /// </summary>
    public List<string> Permissions { get; private set; } = new();

    /// <summary>
    /// System roles cannot be deleted
    /// </summary>
    public bool IsSystem { get; private set; } = false;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<AdminUser> Users { get; set; } = new List<AdminUser>();

    // Domain Methods

    /// <summary>
    /// Creates a new role
    /// </summary>
    public static Role Create(string name, string description, List<string> permissions, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        return new Role
        {
            Name = name,
            Description = description,
            Permissions = permissions ?? new(),
            IsSystem = isSystem,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string description, List<string> permissions)
    {
        if (IsSystem)
            throw new InvalidOperationException("System roles cannot be modified");

        Description = description;
        Permissions = permissions ?? new();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPermission(string permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemovePermission(string permission)
    {
        if (Permissions.Remove(permission))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
