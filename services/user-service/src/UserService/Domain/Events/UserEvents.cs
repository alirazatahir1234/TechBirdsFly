using System;

namespace UserService.Domain.Events;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract record DomainEvent
{
    public Guid AggregateId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when user registers
/// </summary>
public record UserRegisteredEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
}

/// <summary>
/// Event raised when user logs in
/// </summary>
public record UserLoggedInEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public required string Username { get; init; }
    public required DateTime LoginTime { get; init; }
}

/// <summary>
/// Event raised when user profile is updated
/// </summary>
public record ProfileUpdatedEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public required string UpdatedFields { get; init; }
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when user is deactivated
/// </summary>
public record UserDeactivatedEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public required string Reason { get; init; }
    public DateTime DeactivatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when password is changed
/// </summary>
public record PasswordChangedEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when email is verified
/// </summary>
public record EmailVerifiedEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public required string Email { get; init; }
}

/// <summary>
/// Event raised when role is assigned
/// </summary>
public record RoleAssignedEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public required string Role { get; init; }
    public required Guid AssignedBy { get; init; }
    public DateTime AssignedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when account is locked
/// </summary>
public record AccountLockedEvent : DomainEvent
{
    public Guid UserId => AggregateId;
    public required string Reason { get; init; }
    public required DateTime LockoutUntil { get; init; }
}
