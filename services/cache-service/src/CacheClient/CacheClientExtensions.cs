using System;
using Microsoft.Extensions.DependencyInjection;

namespace TechBirdsFly.CacheClient;

/// <summary>
/// Extension methods for registering CacheClient in dependency injection
/// </summary>
public static class CacheClientExtensions
{
    /// <summary>
    /// Add centralized cache client to service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="cacheServiceBaseUrl">Base URL of CacheService (e.g., http://localhost:8100)</param>
    /// <param name="jwtToken">Optional JWT token for authenticated requests</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCacheClient(
        this IServiceCollection services,
        string cacheServiceBaseUrl,
        string? jwtToken = null)
    {
        if (string.IsNullOrWhiteSpace(cacheServiceBaseUrl))
            throw new ArgumentNullException(nameof(cacheServiceBaseUrl));

        services.AddScoped<ICacheClient>(provider =>
        {
            var httpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(cacheServiceBaseUrl.TrimEnd('/'))
            };
            return new HttpCacheClient(httpClient, cacheServiceBaseUrl, jwtToken);
        });

        return services;
    }
}
