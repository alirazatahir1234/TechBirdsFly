namespace TechBirdsFly.Shared.Kernel;

/// <summary>
/// Base entity for all aggregate roots in the domain.
/// Provides common properties like Id, CreatedAt, UpdatedAt.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when entity was created
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; protected set; } = false;

    /// <summary>
    /// Timestamp when entity was deleted (if soft-deleted)
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Collection of domain events that occurred for this entity
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the domain events that have occurred
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity
    /// </summary>
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events (typically after they've been published)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks the entity as deleted (soft delete)
    /// </summary>
    public virtual void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores a soft-deleted entity
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp
    /// </summary>
    public virtual void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Override equality to compare entities by their Id
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity entity)
            return false;

        return Id == entity.Id && entity.GetType() == GetType();
    }

    /// <summary>
    /// Override GetHashCode to use Id
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
