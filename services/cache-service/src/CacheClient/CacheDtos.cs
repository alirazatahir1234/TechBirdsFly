using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TechBirdsFly.CacheClient;

/// <summary>
/// Request/Response DTOs for Cache operations
/// </summary>

public class CacheSetRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int? TtlSeconds { get; set; } // Optional TTL in seconds
    public string? ServiceName { get; set; }
    public string? Category { get; set; }
}

public class CacheGetRequest
{
    public string Key { get; set; } = string.Empty;
}

public class CacheRemoveRequest
{
    public string Key { get; set; } = string.Empty;
}

public class CacheRemovePatternRequest
{
    public string Pattern { get; set; } = string.Empty;
    public string? ServiceName { get; set; }
}

public class CacheResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}

public class CacheValueResponse
{
    public bool Found { get; set; }
    public string? Value { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CacheInvalidationResponse
{
    public bool Success { get; set; }
    public int AffectedCount { get; set; }
    public string? Message { get; set; }
}

public class CacheStatsResponse
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public double HitRatio { get; set; }
    public long MemoryUsageBytes { get; set; }
    public List<ServiceCacheStats> ServiceStats { get; set; } = new();
}

public class ServiceCacheStats
{
    public string ServiceName { get; set; } = string.Empty;
    public long Hits { get; set; }
    public long Misses { get; set; }
    public double HitRatio { get; set; }
}
