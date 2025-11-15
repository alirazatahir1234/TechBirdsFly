using System;
using System.Collections.Generic;

namespace CacheService.Application.DTOs;

// Requests
public class SetCacheRequest
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string? ServiceName { get; set; }
    public string? Category { get; set; }
    public int? TtlSeconds { get; set; }
}

public class GetCacheRequest
{
    public string Key { get; set; }
}

public class RemoveCacheRequest
{
    public string Key { get; set; }
}

public class RemovePatternRequest
{
    public string Pattern { get; set; }
    public string? ServiceName { get; set; }
}

public class CacheStatsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// Responses
public class CacheResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}

public class CacheValueResponse
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string? ServiceName { get; set; }
    public string? Category { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int AccessCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

public class CacheInvalidationResponse
{
    public int AffectedEntriesCount { get; set; }
    public List<string> RemovedKeys { get; set; } = new();
    public string Message { get; set; }
}

public class CacheStatsResponse
{
    public long TotalEntries { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRatio { get; set; }
    public long MemoryUsageBytes { get; set; }
    public List<ServiceCacheStats> ServiceStats { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class ServiceCacheStats
{
    public string ServiceName { get; set; }
    public long EntriesCount { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public long MemoryUsageBytes { get; set; }
}

public class HealthCheckResponse
{
    public string Status { get; set; }
    public string RedisStatus { get; set; }
    public DateTime Timestamp { get; set; }
    public long MemoryUsageBytes { get; set; }
    public Dictionary<string, string> Services { get; set; } = new();
}
