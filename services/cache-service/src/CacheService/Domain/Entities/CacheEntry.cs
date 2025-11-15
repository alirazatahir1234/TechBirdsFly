using System;
using CacheService.Domain.Events;

namespace CacheService.Domain.Entities;

/// <summary>
/// Represents a cache entry in the centralized cache service
/// </summary>
public class CacheEntry
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public byte[] Value { get; set; }
    public string? ServiceName { get; set; }
    public string? Category { get; set; }
    public CacheEntryType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public int AccessCount { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public List<object> DomainEvents { get; } = new();

    private CacheEntry() { }

    public static (bool success, CacheEntry? entry, string? error) Create(
        string key,
        byte[] value,
        string? serviceName = null,
        string? category = null,
        CacheEntryType type = CacheEntryType.Standard,
        TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return (false, null, "Cache key cannot be empty");

        if (value == null || value.Length == 0)
            return (false, null, "Cache value cannot be empty");

        var ttlDuration = ttl ?? TimeSpan.FromHours(1);
        var expiresAt = DateTime.UtcNow.Add(ttlDuration);

        var entry = new CacheEntry
        {
            Id = Guid.NewGuid(),
            Key = key,
            Value = value,
            ServiceName = serviceName,
            Category = category,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            AccessCount = 0
        };

        entry.AddDomainEvent(new CacheEntryCreatedEvent 
        { 
            AggregateId = entry.Id,
            Key = key,
            ServiceName = serviceName,
            Timestamp = DateTime.UtcNow
        });

        return (true, entry, null);
    }

    public void UpdateAccessTime()
    {
        LastAccessedAt = DateTime.UtcNow;
        AccessCount++;

        AddDomainEvent(new CacheAccessedEvent
        {
            AggregateId = Id,
            Key = Key,
            Timestamp = DateTime.UtcNow
        });
    }

    public void RefreshTTL(TimeSpan? ttl = null)
    {
        var ttlDuration = ttl ?? TimeSpan.FromHours(1);
        ExpiresAt = DateTime.UtcNow.Add(ttlDuration);

        AddDomainEvent(new CacheTTLRefreshedEvent
        {
            AggregateId = Id,
            Key = Key,
            NewExpiryTime = ExpiresAt,
            Timestamp = DateTime.UtcNow
        });
    }

    public void MarkForRemoval()
    {
        AddDomainEvent(new CacheEntryRemovedEvent
        {
            AggregateId = Id,
            Key = Key,
            ServiceName = ServiceName,
            Timestamp = DateTime.UtcNow
        });
    }

    private void AddDomainEvent(object domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }
}

public enum CacheEntryType
{
    Standard = 0,
    Session = 1,
    Configuration = 2,
    TransientData = 3
}
