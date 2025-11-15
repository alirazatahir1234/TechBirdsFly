using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TechBirdsFly.CacheClient;

/// <summary>
/// HTTP-based cache client for distributed caching across microservices
/// Communicates with centralized CacheService via REST API
/// </summary>
public class HttpCacheClient : ICacheClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string? _jwtToken;

    /// <summary>
    /// Initialize HttpCacheClient with service endpoint
    /// </summary>
    /// <param name="httpClient">HttpClient configured for cache service communication</param>
    /// <param name="cacheServiceBaseUrl">Base URL of CacheService (e.g., http://localhost:8100)</param>
    /// <param name="jwtToken">Optional JWT token for authenticated requests</param>
    public HttpCacheClient(HttpClient httpClient, string cacheServiceBaseUrl, string? jwtToken = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = cacheServiceBaseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(cacheServiceBaseUrl));
        _jwtToken = jwtToken;
    }

    public async Task<(bool success, CacheResponse? response, string? error)> SetAsync(
        string key, string value, TimeSpan? ttl = null, string? serviceName = null, string? category = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, null, "Cache key cannot be empty");

            var request = new CacheSetRequest
            {
                Key = key,
                Value = value,
                TtlSeconds = (int?)ttl?.TotalSeconds,
                ServiceName = serviceName,
                Category = category
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/cache/set")
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(_jwtToken))
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var cacheResponse = JsonSerializer.Deserialize<CacheResponse>(responseContent);
                return (true, cacheResponse, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, null, $"Failed to set cache: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, CacheValueResponse? response, string? error)> GetAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, null, "Cache key cannot be empty");

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/cache/get/{Uri.EscapeDataString(key)}");

            if (!string.IsNullOrEmpty(_jwtToken))
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var cacheResponse = JsonSerializer.Deserialize<CacheValueResponse>(responseContent);
                return (true, cacheResponse, null);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return (true, new CacheValueResponse { Found = false }, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, null, $"Failed to get cache: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, CacheResponse? response, string? error)> RemoveAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return (false, null, "Cache key cannot be empty");

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/api/cache/remove/{Uri.EscapeDataString(key)}");

            if (!string.IsNullOrEmpty(_jwtToken))
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var cacheResponse = JsonSerializer.Deserialize<CacheResponse>(responseContent);
                return (true, cacheResponse, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, null, $"Failed to remove cache: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, CacheInvalidationResponse? response, string? error)> RemovePatternAsync(string pattern, string? serviceName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return (false, null, "Pattern cannot be empty");

            var request = new CacheRemovePatternRequest
            {
                Pattern = pattern,
                ServiceName = serviceName
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/cache/remove-pattern")
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(_jwtToken))
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var cacheResponse = JsonSerializer.Deserialize<CacheInvalidationResponse>(responseContent);
                return (true, cacheResponse, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, null, $"Failed to remove pattern: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, CacheStatsResponse? response, string? error)> GetStatsAsync()
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/cache/stats");

            if (!string.IsNullOrEmpty(_jwtToken))
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var cacheResponse = JsonSerializer.Deserialize<CacheStatsResponse>(responseContent);
                return (true, cacheResponse, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, null, $"Failed to get stats: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool healthy, string? reason)> HealthCheckAsync()
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/cache/health");

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            return (false, $"Cache service unhealthy: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
