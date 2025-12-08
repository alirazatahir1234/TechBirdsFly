using Microsoft.AspNetCore.Mvc;
using MediatR;
using GeneratorService.Application.DTOs;
using GeneratorService.Application.Features.GenerateWebsite;
using GeneratorService.WebAPI.Extensions;

namespace GeneratorService.WebAPI.Controllers;

/// <summary>
/// API controller for website generation operations
/// Orchestrates AI-powered website generation with comprehensive error handling
/// </summary>
[ApiController]
[Route("api/[controller]/v1")]
[Produces("application/json")]
public class GenerateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateController> _logger;

    public GenerateController(IMediator mediator, ILogger<GenerateController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Generates a complete website based on provided specifications
    /// Integrates AI (Llama3) to create HTML, CSS, and sections
    /// </summary>
    /// <param name="command">Website generation request with industry, style, colors, and prompt</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Generated website with HTML, sections, metadata, and styling</returns>
    /// <response code="200">Website generated successfully</response>
    /// <response code="400">Invalid request parameters or validation failure</response>
    /// <response code="500">Server error during generation (AI service unavailable or internal error)</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GeneratedWebsiteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateWebsite(
        [FromBody] GenerateWebsiteCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Website generation requested: {ProjectName} ({Industry})",
                command.ProjectName, command.Industry);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Website generated successfully: {ProjectName}", command.ProjectName);

            return Ok(result.ToApiResponse("Website generated successfully"));
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new
            {
                success = false,
                errors = ex.Errors.Select(e => e.ErrorMessage),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating website: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new
                {
                    success = false,
                    error = "An error occurred while generating the website",
                    timestamp = DateTime.UtcNow
                });
        }
    }

    /// <summary>
    /// Health check endpoint for the generator service
    /// </summary>
    /// <returns>Service health status with timestamp</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            success = true,
            status = "healthy",
            service = "GeneratorService",
            version = "1.0.0",
            timestamp = DateTime.UtcNow
        });
    }
}
