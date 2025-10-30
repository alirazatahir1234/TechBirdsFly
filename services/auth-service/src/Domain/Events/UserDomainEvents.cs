using TechBirdsFly.Shared.Kernel;

namespace AuthService.Domain.Events;

public class UserCreatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserCreatedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}

public class UserEmailConfirmedDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserEmailConfirmedDomainEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}

public class UserDeactivatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserDeactivatedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}

public class UserActivatedDomainEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserActivatedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}

public class UserLoginDomainEvent : DomainEvent
{
    public Guid UserId { get; }
    public DateTime LoginAt { get; }
    public string IpAddress { get; }

    public UserLoginDomainEvent(Guid userId, DateTime loginAt, string ipAddress)
    {
        UserId = userId;
        LoginAt = loginAt;
        IpAddress = ipAddress;
    }
}
