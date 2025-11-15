using System;
using TechBirdsFly.Shared.Kernel;

namespace CacheService.Domain.Events;

public abstract class CacheEventBase : DomainEvent
{
    public Guid AggregateId { get; set; }
    public string Key { get; set; }
    public DateTime Timestamp { get; set; }
}

public class CacheEntryCreatedEvent : CacheEventBase
{
    public string? ServiceName { get; set; }
}

public class CacheAccessedEvent : CacheEventBase
{
    public int AccessCount { get; set; }
}

public class CacheTTLRefreshedEvent : CacheEventBase
{
    public DateTime NewExpiryTime { get; set; }
}

public class CacheEntryRemovedEvent : CacheEventBase
{
    public string? ServiceName { get; set; }
    public string? Reason { get; set; }
}

public class CacheInvalidatedEvent : CacheEventBase
{
    public string? Pattern { get; set; }
    public int AffectedEntriesCount { get; set; }
    public string? InitiatedBy { get; set; }
}

public class CacheStatsUpdatedEvent : CacheEventBase
{
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRatio { get; set; }
}
