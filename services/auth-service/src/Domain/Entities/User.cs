using TechBirdsFly.Shared.Kernel;
using AuthService.Domain.Events;

namespace AuthService.Domain.Entities;

/// <summary>
/// User aggregate root for authentication context
/// </summary>
public class User : BaseEntity, IAggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private User() { }

    // Factory method
    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            IsEmailConfirmed = false,
            IsActive = true
        };

        // Raise domain event
        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, email));

        return user;
    }

    public void ConfirmEmail()
    {
        if (IsEmailConfirmed)
            throw new InvalidOperationException("Email is already confirmed");

        IsEmailConfirmed = true;
        UpdateTimestamp();
        RaiseDomainEvent(new UserEmailConfirmedDomainEvent(Id, Email));
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already deactivated");

        IsActive = false;
        UpdateTimestamp();
        RaiseDomainEvent(new UserDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active");

        IsActive = true;
        UpdateTimestamp();
        RaiseDomainEvent(new UserActivatedDomainEvent(Id));
    }
}
