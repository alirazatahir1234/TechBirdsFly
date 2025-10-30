using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.Cache
{
    /// <summary>
    /// Redis-based implementation of ICacheService
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get a cached value by key
        /// </summary>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cached = await _cache.GetStringAsync(key, cancellationToken);
                if (cached is null)
                {
                    _logger.LogDebug($"Cache miss for key: {key}");
                    return default;
                }

                var result = JsonSerializer.Deserialize<T>(cached);
                _logger.LogDebug($"Cache hit for key: {key}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving cache key {key}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Set a value in cache with optional expiry
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
                };

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options, cancellationToken);
                _logger.LogDebug($"Cache set for key: {key} with expiry: {expiration?.TotalMinutes ?? 10} minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting cache key {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove a value from cache
        /// </summary>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug($"Cache removed for key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing cache key {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a key exists in cache
        /// </summary>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                return value is not null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking cache key existence {key}: {ex.Message}");
                return false;
            }
        }
    }
}
