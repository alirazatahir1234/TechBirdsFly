using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AuthService.Services.Cache
{
    /// <summary>
    /// Interface for distributed caching service using Redis
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Get a value from cache
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Set a value in cache
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// Remove a value from cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Check if a key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key);
    }

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
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cached = await _cache.GetStringAsync(key);
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
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
                };

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);
                _logger.LogDebug($"Cache set for key: {key} with expiry: {expiry?.TotalMinutes ?? 10} minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting cache key {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove a value from cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
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
        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
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
