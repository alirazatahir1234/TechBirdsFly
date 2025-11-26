namespace GeneratorService.Domain.Common;

/// <summary>
/// Base entity class for all domain entities
/// Provides unique identity via Guid
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    protected BaseEntity()
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
