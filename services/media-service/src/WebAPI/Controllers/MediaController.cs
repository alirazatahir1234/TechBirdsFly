using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechBirdsFly.MediaService.Application.Features.DeleteImage;
using TechBirdsFly.MediaService.Application.Features.GenerateImage;
using TechBirdsFly.MediaService.Application.Features.GetImage;
using TechBirdsFly.MediaService.Application.Features.UploadImage;
using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.WebAPI.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediaController> _logger;
    private readonly IScreenshotService _screenshots;

    public MediaController(IMediator mediator, ILogger<MediaController> logger, IScreenshotService screenshots)
    {
        _mediator = mediator;
        _logger = logger;
        _screenshots = screenshots;
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> UploadImage(IFormFile file)
    {
        _logger.LogInformation("Uploading image: {FileName}", file.FileName);

        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        using var stream = file.OpenReadStream();
        var command = new UploadImageCommand(stream, file.FileName, file.ContentType, file.Length);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(GenerateImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerateImageResponse>> GenerateImage([FromBody] GenerateImageDto dto)
    {
        _logger.LogInformation("Generating image with prompt: {Prompt}", dto.Prompt);

        var command = new GenerateImageCommand(dto.Prompt, dto.Style, dto.Width, dto.Height);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetImageResponse>> GetImage(Guid id)
    {
        _logger.LogInformation("Getting image: {Id}", id);

        var query = new GetImageQuery(id);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteImage(Guid id)
    {
        _logger.LogInformation("Deleting image: {Id}", id);

        var command = new DeleteImageCommand(id);
        await _mediator.Send(command);

        return Ok();
    }

    [HttpPost("screenshot")]
    [ProducesResponseType(typeof(ScreenshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScreenshotResponse>> CaptureScreenshot([FromBody] ScreenshotRequest request)
    {
        _logger.LogInformation("Capturing screenshot for HTML content");

        if (string.IsNullOrEmpty(request.Html))
            return BadRequest("HTML content is required");

        try
        {
            var screenshotBytes = await _screenshots.CaptureAsync(request.Html);
            var base64 = Convert.ToBase64String(screenshotBytes);

            return Ok(new ScreenshotResponse
            {
                Base64 = base64,
                Size = screenshotBytes.Length,
                CapturedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture screenshot");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to capture screenshot", message = ex.Message });
        }
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new { status = "Media Service is healthy", timestamp = DateTime.UtcNow });
    }
}

public class GenerateImageDto
{
    public string Prompt { get; set; } = null!;
    public string? Style { get; set; }
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
}

public class ScreenshotRequest
{
    public string Html { get; set; } = null!;
}

public class ScreenshotResponse
{
    public string Base64 { get; set; } = null!;
    public int Size { get; set; }
    public DateTime CapturedAt { get; set; }
}
