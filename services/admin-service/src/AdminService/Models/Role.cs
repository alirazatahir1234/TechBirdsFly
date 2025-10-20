namespace AdminService.Models;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // Admin, Creator, Viewer, etc.
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new(); // Comma-separated or JSON
    public bool IsSystem { get; set; } = false; // System roles can't be deleted
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
}
