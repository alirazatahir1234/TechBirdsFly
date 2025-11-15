using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheService.Application.DTOs;
using CacheService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CacheService.WebAPI.Controllers;

/// <summary>
/// Central Cache Service API Controller
/// Provides REST endpoints for cache operations, statistics, and health monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CacheController : ControllerBase
{
    private readonly ICacheApplicationService _cacheApplicationService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(
        ICacheApplicationService cacheApplicationService,
        ILogger<CacheController> logger)
    {
        _cacheApplicationService = cacheApplicationService ?? throw new ArgumentNullException(nameof(cacheApplicationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Set a cache entry
    /// </summary>
    /// <param name="request">Set cache request with key, value, and optional TTL</param>
    /// <returns>Response indicating success or failure</returns>
    [HttpPost("set")]
    [ProducesResponseType(typeof(CacheResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CacheResponse>> SetCacheAsync([FromBody] SetCacheRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, response, error) = await _cacheApplicationService.SetCacheAsync(request);

            if (!success)
                return BadRequest(new CacheResponse { Success = false, Message = error ?? "Unknown error" });

            _logger.LogInformation($"Cache set: {request.Key}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache");
            return StatusCode(500, new CacheResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Get a cache entry by key
    /// </summary>
    /// <param name="key">The cache key to retrieve</param>
    /// <returns>Cache entry with value and metadata</returns>
    [HttpGet("get/{key}")]
    [ProducesResponseType(typeof(CacheValueResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CacheValueResponse>> GetCacheAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest(new { error = "Cache key cannot be empty" });

            var (success, response, error) = await _cacheApplicationService.GetCacheAsync(key);

            if (!success)
                return NotFound(new { error = error ?? "Key not found" });

            _logger.LogDebug($"Cache retrieved: {key}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving cache: {key}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove a cache entry by key
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <returns>Response indicating success or failure</returns>
    [HttpDelete("remove/{key}")]
    [ProducesResponseType(typeof(CacheResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CacheResponse>> RemoveCacheAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest(new CacheResponse { Success = false, Message = "Cache key cannot be empty" });

            var (success, response, error) = await _cacheApplicationService.RemoveCacheAsync(key);

            if (!success)
                return NotFound(new CacheResponse { Success = false, Message = error ?? "Key not found" });

            _logger.LogInformation($"Cache removed: {key}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache: {key}");
            return StatusCode(500, new CacheResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Remove cache entries matching a pattern
    /// </summary>
    /// <param name="request">Pattern removal request with pattern and optional service name</param>
    /// <returns>Response with count of removed entries</returns>
    [HttpPost("remove-pattern")]
    [ProducesResponseType(typeof(CacheInvalidationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CacheInvalidationResponse>> RemovePatternAsync([FromBody] RemovePatternRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Pattern))
                return BadRequest(new CacheInvalidationResponse { Message = "Pattern cannot be empty" });

            var (success, response, error) = await _cacheApplicationService.RemoveByPatternAsync(request.Pattern, request.ServiceName);

            if (!success)
                return BadRequest(new CacheInvalidationResponse { Message = error ?? "Failed to remove pattern" });

            _logger.LogInformation($"Cache pattern removed: {request.Pattern} (count: {response?.AffectedEntriesCount ?? 0})");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache pattern: {request.Pattern}");
            return StatusCode(500, new CacheInvalidationResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Get cache statistics and performance metrics
    /// </summary>
    /// <returns>Cache statistics including hit/miss ratios and service breakdowns</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CacheStatsResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CacheStatsResponse>> GetStatsAsync()
    {
        try
        {
            var (success, response, error) = await _cacheApplicationService.GetStatsAsync();

            if (!success)
                return StatusCode(500, new { error = error ?? "Failed to retrieve stats" });

            _logger.LogDebug("Cache statistics retrieved");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint for service monitoring
    /// </summary>
    /// <returns>Health status including Redis connection status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthCheckResponse), 200)]
    public async Task<ActionResult<HealthCheckResponse>> HealthCheckAsync()
    {
        try
        {
            var (success, response, error) = await _cacheApplicationService.CheckHealthAsync();

            if (!success)
                return StatusCode(503, new { status = "Unhealthy", error = error ?? "Health check failed" });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new { status = "Unhealthy", error = ex.Message });
        }
    }
}
