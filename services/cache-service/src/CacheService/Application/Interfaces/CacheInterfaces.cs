using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheService.Application.DTOs;

namespace CacheService.Application.Interfaces;

public interface IRedisCacheService
{
    Task<(bool success, string? value, string? error)> GetAsync(string key);
    Task<(bool success, string? error)> SetAsync(string key, string value, TimeSpan? ttl = null);
    Task<(bool success, string? error)> RemoveAsync(string key);
    Task<(bool success, int count, string? error)> RemoveByPatternAsync(string pattern);
    Task<(bool success, string? error)> ClearAllAsync();
    Task<(bool success, bool exists, string? error)> ExistsAsync(string key);
    Task<(bool success, CacheStatsResponse? stats, string? error)> GetStatsAsync();
}

public interface ICacheApplicationService
{
    Task<(bool success, CacheResponse? response, string? error)> SetCacheAsync(SetCacheRequest request);
    Task<(bool success, CacheValueResponse? response, string? error)> GetCacheAsync(string key);
    Task<(bool success, CacheResponse? response, string? error)> RemoveCacheAsync(string key);
    Task<(bool success, CacheInvalidationResponse? response, string? error)> RemoveByPatternAsync(string pattern, string? serviceName = null);
    Task<(bool success, CacheStatsResponse? response, string? error)> GetStatsAsync();
    Task<(bool success, HealthCheckResponse? response, string? error)> CheckHealthAsync();
}

public interface IKafkaEventConsumer
{
    Task StartAsync();
    Task StopAsync();
    Task HandleCacheInvalidationEventAsync(string message);
}

public interface IMetricsService
{
    void RecordCacheHit(string key, string? serviceName = null);
    void RecordCacheMiss(string key, string? serviceName = null);
    void RecordCacheSet(string key, long sizeBytes, string? serviceName = null);
    void RecordCacheRemove(string key, string? serviceName = null);
    Task<CacheStatsResponse> GetMetricsAsync();
}
