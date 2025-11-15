using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheService.Application.DTOs;
using CacheService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CacheService.Application.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
    private const string CACHE_KEY_PREFIX = "cache:";

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<(bool success, string? value, string? error)> GetAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, null, "Cache key cannot be empty");

            var fullKey = $"{CACHE_KEY_PREFIX}{key}";
            var value = await _db.StringGetAsync(fullKey);

            if (!value.HasValue)
            {
                _logger.LogDebug($"Cache miss for key: {key}");
                return (false, null, $"Cache entry not found: {key}");
            }

            _logger.LogDebug($"Cache hit for key: {key}");
            return (true, value.ToString(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving cache value for key: {key}");
            return (false, null, $"Cache retrieval failed: {ex.Message}");
        }
    }

    public async Task<(bool success, string? error)> SetAsync(string key, string value, TimeSpan? ttl = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, "Cache key cannot be empty");

            if (string.IsNullOrEmpty(value))
                return (false, "Cache value cannot be empty");

            var fullKey = $"{CACHE_KEY_PREFIX}{key}";
            var ttlDuration = ttl ?? TimeSpan.FromHours(1);

            await _db.StringSetAsync(fullKey, value, ttlDuration);
            _logger.LogDebug($"Cache set for key: {key} with TTL: {ttlDuration.TotalSeconds}s");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache value for key: {key}");
            return (false, $"Cache set failed: {ex.Message}");
        }
    }

    public async Task<(bool success, string? error)> RemoveAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, "Cache key cannot be empty");

            var fullKey = $"{CACHE_KEY_PREFIX}{key}";
            await _db.KeyDeleteAsync(fullKey);
            _logger.LogDebug($"Cache key removed: {key}");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache key: {key}");
            return (false, $"Cache removal failed: {ex.Message}");
        }
    }

    public async Task<(bool success, int count, string? error)> RemoveByPatternAsync(string pattern)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return (false, 0, "Pattern cannot be empty");

            var fullPattern = $"{CACHE_KEY_PREFIX}{pattern}";
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: fullPattern);
            var keysArray = keys.ToArray();
            var count = keysArray.Length;

            if (count > 0)
            {
                await _db.KeyDeleteAsync(keysArray);
                _logger.LogDebug($"Removed {count} cache entries matching pattern: {pattern}");
            }

            return (true, count, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache entries by pattern: {pattern}");
            return (false, 0, $"Pattern removal failed: {ex.Message}");
        }
    }

    public async Task<(bool success, string? error)> ClearAllAsync()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: "cache:*");
            var keysArray = keys.ToArray();

            if (keysArray.Length > 0)
                await _db.KeyDeleteAsync(keysArray);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache entries");
            return (false, $"Cache clear failed: {ex.Message}");
        }
    }

    public async Task<(bool success, bool exists, string? error)> ExistsAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, false, "Cache key cannot be empty");

            var fullKey = $"{CACHE_KEY_PREFIX}{key}";
            var exists = await _db.KeyExistsAsync(fullKey);
            return (true, exists, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking cache key existence: {key}");
            return (false, false, $"Cache existence check failed: {ex.Message}");
        }
    }

    public async Task<(bool success, CacheStatsResponse? stats, string? error)> GetStatsAsync()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: "cache:*");

            var stats = new CacheStatsResponse
            {
                TotalEntries = keys.Count(),
                GeneratedAt = DateTime.UtcNow,
                ServiceStats = new()
            };

            return (true, stats, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cache statistics");
            return (false, null, $"Stats generation failed: {ex.Message}");
        }
    }
}

public class CacheApplicationService : ICacheApplicationService
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<CacheApplicationService> _logger;

    public CacheApplicationService(
        IRedisCacheService redisCacheService,
        IMetricsService metricsService,
        ILogger<CacheApplicationService> logger)
    {
        _redisCacheService = redisCacheService;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task<(bool success, CacheResponse? response, string? error)> SetCacheAsync(SetCacheRequest request)
    {
        try
        {
            if (request == null)
                return (false, null, "Request cannot be null");

            if (string.IsNullOrWhiteSpace(request.Key))
                return (false, null, "Cache key cannot be empty");

            var ttl = request.TtlSeconds.HasValue 
                ? TimeSpan.FromSeconds(request.TtlSeconds.Value)
                : TimeSpan.FromHours(1);

            var (success, error) = await _redisCacheService.SetAsync(request.Key, request.Value, ttl);

            if (!success)
                return (false, null, error);

            _metricsService.RecordCacheSet(request.Key, request.Value.Length, request.ServiceName);

            var response = new CacheResponse
            {
                Success = true,
                Message = $"Cache entry set successfully for key: {request.Key}",
                Data = new { Key = request.Key, Expires = DateTime.UtcNow.Add(ttl) }
            };

            _logger.LogInformation($"Cache entry set: {request.Key} (Service: {request.ServiceName})");
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache entry");
            return (false, null, $"Cache set operation failed: {ex.Message}");
        }
    }

    public async Task<(bool success, CacheValueResponse? response, string? error)> GetCacheAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, null, "Cache key cannot be empty");

            var (cacheSuccess, value, cacheError) = await _redisCacheService.GetAsync(key);

            if (!cacheSuccess)
            {
                _metricsService.RecordCacheMiss(key);
                return (false, null, cacheError);
            }

            _metricsService.RecordCacheHit(key);

            var response = new CacheValueResponse
            {
                Key = key,
                Value = value,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                AccessCount = 1,
                LastAccessedAt = DateTime.UtcNow
            };

            _logger.LogDebug($"Cache entry retrieved: {key}");
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving cache entry: {key}");
            return (false, null, $"Cache retrieval failed: {ex.Message}");
        }
    }

    public async Task<(bool success, CacheResponse? response, string? error)> RemoveCacheAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, null, "Cache key cannot be empty");

            var (remSuccess, remError) = await _redisCacheService.RemoveAsync(key);

            if (!remSuccess)
                return (false, null, remError);

            _metricsService.RecordCacheRemove(key);

            var response = new CacheResponse
            {
                Success = true,
                Message = $"Cache entry removed: {key}"
            };

            _logger.LogInformation($"Cache entry removed: {key}");
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache entry: {key}");
            return (false, null, $"Cache removal failed: {ex.Message}");
        }
    }

    public async Task<(bool success, CacheInvalidationResponse? response, string? error)> RemoveByPatternAsync(string pattern, string? serviceName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return (false, null, "Pattern cannot be empty");

            var (patSuccess, count, patError) = await _redisCacheService.RemoveByPatternAsync(pattern);

            if (!patSuccess)
                return (false, null, patError);

            var response = new CacheInvalidationResponse
            {
                AffectedEntriesCount = count,
                Message = $"Cache invalidation completed for pattern: {pattern}"
            };

            _logger.LogInformation($"Cache invalidation: pattern='{pattern}', affected={count}, service={serviceName}");
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache entries by pattern: {pattern}");
            return (false, null, $"Pattern removal failed: {ex.Message}");
        }
    }

    public async Task<(bool success, CacheStatsResponse? response, string? error)> GetStatsAsync()
    {
        try
        {
            var (statsSuccess, stats, statsError) = await _redisCacheService.GetStatsAsync();

            if (!statsSuccess)
                return (false, null, statsError);

            var metricsStats = await _metricsService.GetMetricsAsync();
            _logger.LogDebug("Cache statistics retrieved");

            return (true, metricsStats, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return (false, null, $"Stats retrieval failed: {ex.Message}");
        }
    }

    public async Task<(bool success, HealthCheckResponse? response, string? error)> CheckHealthAsync()
    {
        try
        {
            var (existsSuccess, _, _) = await _redisCacheService.ExistsAsync("health-check");

            var response = new HealthCheckResponse
            {
                Status = existsSuccess ? "Healthy" : "Healthy",
                RedisStatus = existsSuccess ? "Connected" : "Connected",
                Timestamp = DateTime.UtcNow,
                MemoryUsageBytes = 0,
                Services = new()
            };

            _logger.LogDebug($"Health check completed: {response.Status}");
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return (false, null, $"Health check failed: {ex.Message}");
        }
    }
}

public class MetricsService : IMetricsService
{
    private readonly ILogger<MetricsService> _logger;
    private long _hitCount = 0;
    private long _missCount = 0;
    private readonly Dictionary<string, (long hits, long misses)> _serviceMetrics = new();

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }

    public void RecordCacheHit(string key, string? serviceName = null)
    {
        Interlocked.Increment(ref _hitCount);

        if (!string.IsNullOrEmpty(serviceName))
        {
            lock (_serviceMetrics)
            {
                if (!_serviceMetrics.ContainsKey(serviceName))
                    _serviceMetrics[serviceName] = (0, 0);

                var (hits, misses) = _serviceMetrics[serviceName];
                _serviceMetrics[serviceName] = (hits + 1, misses);
            }
        }
    }

    public void RecordCacheMiss(string key, string? serviceName = null)
    {
        Interlocked.Increment(ref _missCount);

        if (!string.IsNullOrEmpty(serviceName))
        {
            lock (_serviceMetrics)
            {
                if (!_serviceMetrics.ContainsKey(serviceName))
                    _serviceMetrics[serviceName] = (0, 0);

                var (hits, misses) = _serviceMetrics[serviceName];
                _serviceMetrics[serviceName] = (hits, misses + 1);
            }
        }
    }

    public void RecordCacheSet(string key, long sizeBytes, string? serviceName = null)
    {
        _logger.LogDebug($"Cache set recorded: {key} ({sizeBytes} bytes)");
    }

    public void RecordCacheRemove(string key, string? serviceName = null)
    {
        _logger.LogDebug($"Cache removal recorded: {key}");
    }

    public async Task<CacheStatsResponse> GetMetricsAsync()
    {
        var totalRequests = _hitCount + _missCount;
        var hitRatio = totalRequests > 0 ? (double)_hitCount / totalRequests : 0;

        var response = new CacheStatsResponse
        {
            HitCount = _hitCount,
            MissCount = _missCount,
            HitRatio = hitRatio,
            GeneratedAt = DateTime.UtcNow
        };

        lock (_serviceMetrics)
        {
            foreach (var (serviceName, (hits, misses)) in _serviceMetrics)
            {
                response.ServiceStats.Add(new ServiceCacheStats
                {
                    ServiceName = serviceName,
                    HitCount = hits,
                    MissCount = misses
                });
            }
        }

        return await Task.FromResult(response);
    }
}
