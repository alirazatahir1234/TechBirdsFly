namespace TechBirdsFly.Shared.Kernel;

/// <summary>
/// Marker interface for aggregate roots in Domain-Driven Design.
/// Aggregate roots are the entry point to aggregates and manage consistency within the aggregate.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the unique identifier of the aggregate root
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the domain events that have been raised by this aggregate
    /// </summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears the domain events after they have been processed
    /// </summary>
    void ClearDomainEvents();
}
