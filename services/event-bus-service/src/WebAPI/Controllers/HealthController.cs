using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EventBusService.WebAPI.Controllers;

/// <summary>
/// Health check and system information endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get service status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("Health check requested");
        return Ok(new
        {
            service = "Event Bus Service",
            version = "1.0.0",
            status = "healthy",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get service information
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            name = "TechBirdsFly Event Bus Service",
            description = "Distributed event bus with Kafka & Outbox pattern",
            version = "1.0.0",
            endpoints = new
            {
                events = "/api/events",
                subscriptions = "/api/subscriptions",
                health = "/health"
            }
        });
    }
}
