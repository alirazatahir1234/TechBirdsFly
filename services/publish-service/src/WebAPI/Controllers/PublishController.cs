using MediatR;
using Microsoft.AspNetCore.Mvc;
using PublishService.Application.Commands;
using PublishService.Application.DTOs;
using PublishService.Domain.Interfaces;

namespace PublishService.WebAPI.Controllers;

/// <summary>
/// Controller for website publishing operations
/// </summary>
[ApiController]
[Route("api/publish")]
public class PublishController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPublishRepository _repository;

    public PublishController(IMediator mediator, IPublishRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    /// <summary>
    /// Deploy a website to Vercel/Netlify/TechBirdsFly
    /// </summary>
    [HttpPost("deploy")]
    [ProducesResponseType(typeof(DeployResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Deploy([FromBody] DeployRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Html))
            return BadRequest("HTML content is required");

        if (string.IsNullOrWhiteSpace(dto.Provider))
            return BadRequest("Provider is required (vercel, netlify, or techbirdsfly)");

        try
        {
            var url = await _mediator.Send(new DeployCommand(dto));
            var latest = await _repository.GetLatestByProjectAsync(dto.ProjectId);

            return Ok(new DeployResponseDto
            {
                PublishRecordId = latest!.Id,
                Url = url,
                Status = "SUCCESS"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get deployment status
    /// </summary>
    [HttpGet("status/{recordId:guid}")]
    [ProducesResponseType(typeof(PublishStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid recordId)
    {
        var record = await _repository.GetByIdAsync(recordId);
        if (record == null)
            return NotFound();

        return Ok(new PublishStatusDto
        {
            Id = record.Id,
            ProjectId = record.ProjectId,
            Provider = record.Provider,
            Url = record.Url ?? "",
            Status = record.Status,
            ErrorMessage = record.ErrorMessage,
            CreatedAt = record.CreatedAt,
            CompletedAt = record.CompletedAt
        });
    }

    /// <summary>
    /// Get project publish history
    /// </summary>
    [HttpGet("history/{projectId:guid}")]
    [ProducesResponseType(typeof(PublishHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid projectId, [FromQuery] int limit = 20)
    {
        var records = await _repository.GetByProjectAsync(projectId, Math.Min(limit, 100));

        var history = new PublishHistoryDto
        {
            Records = records.Select(r => new PublishStatusDto
            {
                Id = r.Id,
                ProjectId = r.ProjectId,
                Provider = r.Provider,
                Url = r.Url ?? "",
                Status = r.Status,
                ErrorMessage = r.ErrorMessage,
                CreatedAt = r.CreatedAt,
                CompletedAt = r.CompletedAt
            }).ToList(),
            Total = records.Count
        };

        return Ok(history);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
