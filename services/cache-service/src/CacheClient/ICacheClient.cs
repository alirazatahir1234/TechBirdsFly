using System;
using System.Threading.Tasks;

namespace TechBirdsFly.CacheClient;

/// <summary>
/// Interface for distributed cache operations
/// Implemented by HTTP client wrapper connecting to centralized CacheService
/// </summary>
public interface ICacheClient
{
    /// <summary>
    /// Set a cache entry with optional TTL
    /// </summary>
    Task<(bool success, CacheResponse? response, string? error)> SetAsync(string key, string value, TimeSpan? ttl = null, string? serviceName = null, string? category = null);

    /// <summary>
    /// Get a cache entry by key
    /// </summary>
    Task<(bool success, CacheValueResponse? response, string? error)> GetAsync(string key);

    /// <summary>
    /// Remove a cache entry by key
    /// </summary>
    Task<(bool success, CacheResponse? response, string? error)> RemoveAsync(string key);

    /// <summary>
    /// Remove all cache entries matching a pattern
    /// </summary>
    Task<(bool success, CacheInvalidationResponse? response, string? error)> RemovePatternAsync(string pattern, string? serviceName = null);

    /// <summary>
    /// Get cache statistics and performance metrics
    /// </summary>
    Task<(bool success, CacheStatsResponse? response, string? error)> GetStatsAsync();

    /// <summary>
    /// Check health of cache service connection
    /// </summary>
    Task<(bool healthy, string? reason)> HealthCheckAsync();
}
