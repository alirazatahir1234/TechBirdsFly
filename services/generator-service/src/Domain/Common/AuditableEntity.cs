namespace GeneratorService.Domain.Common;

/// <summary>
/// Auditable entity class for entities that need creation/update tracking
/// Extends BaseEntity with audit timestamps
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    protected AuditableEntity() : base()
    {
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp to current UTC time
    /// Call this when making changes to the entity
    /// </summary>
    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
