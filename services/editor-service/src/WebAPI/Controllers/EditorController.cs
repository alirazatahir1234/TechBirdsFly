using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechBirdsFly.EditorService.Application.Features.CreateSection;
using TechBirdsFly.EditorService.Application.Features.DeleteSection;
using TechBirdsFly.EditorService.Application.Features.ListSections;
using TechBirdsFly.EditorService.Application.Features.RegenerateSection;
using TechBirdsFly.EditorService.Application.Features.UpdateSection;

namespace TechBirdsFly.EditorService.WebAPI.Controllers;

[ApiController]
[Route("api/editor")]
public class EditorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EditorController> _logger;

    public EditorController(IMediator mediator, ILogger<EditorController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all sections for a project
    /// </summary>
    [HttpGet("{projectId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListSections(Guid projectId)
    {
        _logger.LogInformation("Listing sections for project: {ProjectId}", projectId);
        var sections = await _mediator.Send(new ListSectionsQuery(projectId));
        return Ok(sections);
    }

    /// <summary>
    /// Create a new section
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionCommand cmd)
    {
        _logger.LogInformation("Creating section for project: {ProjectId}", cmd.ProjectId);
        var sectionId = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(ListSections), new { projectId = cmd.ProjectId }, new { id = sectionId });
    }

    /// <summary>
    /// Update section content
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSection(Guid id, [FromBody] UpdateSectionCommand cmd)
    {
        _logger.LogInformation("Updating section: {SectionId}", id);
        await _mediator.Send(cmd with { Id = id });
        return Ok(new { message = "Section updated successfully" });
    }

    /// <summary>
    /// Delete a section
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection(Guid id)
    {
        _logger.LogInformation("Deleting section: {SectionId}", id);
        await _mediator.Send(new DeleteSectionCommand(id));
        return Ok(new { message = "Section deleted successfully" });
    }

    /// <summary>
    /// Regenerate section HTML using AI
    /// </summary>
    [HttpPost("regenerate/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegenerateSection(Guid id)
    {
        _logger.LogInformation("Regenerating section: {SectionId}", id);
        try
        {
            var newHtml = await _mediator.Send(new RegenerateSectionCommand(id));
            return Ok(new { html = newHtml, message = "Section regenerated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating section {SectionId}", id);
            return StatusCode(500, new { message = "Error regenerating section", error = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
