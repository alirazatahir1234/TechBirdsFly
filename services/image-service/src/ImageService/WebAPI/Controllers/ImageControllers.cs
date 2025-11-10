using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImageService.Application.DTOs;
using ImageService.Application.Interfaces;

namespace ImageService.WebAPI.Controllers;

/// <summary>
/// API Controller for image management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ImagesController : ControllerBase
{
    private readonly IImageUploadService _uploadService;
    private readonly IImageMetadataService _metadataService;
    private readonly ICdnService _cdnService;

    public ImagesController(
        IImageUploadService uploadService,
        IImageMetadataService metadataService,
        ICdnService cdnService)
    {
        _uploadService = uploadService;
        _metadataService = metadataService;
        _cdnService = cdnService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Upload a new image
    /// </summary>
    /// <param name="request">Upload request containing image data</param>
    /// <returns>Created image with 201 status</returns>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<ImageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ImageDto>>> UploadImage([FromBody] UploadImageRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _uploadService.UploadImageAsync(request, userId);
        
        if (result.Success)
            return CreatedAtAction(nameof(GetImage), new { imageId = result.Data?.Id }, result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get image details by ID
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <returns>Image details</returns>
    [HttpGet("{imageId}")]
    [ProducesResponseType(typeof(ApiResponse<ImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ImageDto>>> GetImage(Guid imageId)
    {
        var result = await _uploadService.GetImageAsync(imageId);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// List all images for current user with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="sortBy">Sort by field (default: CreatedAt)</param>
    /// <param name="ascending">Sort order (default: false)</param>
    /// <returns>Paginated list of images</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ImageListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ImageListItemDto>>>> ListImages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new ListImagesQuery(pageNumber, pageSize, sortBy, ascending);
        var result = await _uploadService.ListUserImagesAsync(userId, query);
        
        return Ok(result);
    }

    /// <summary>
    /// Get all images in an album
    /// </summary>
    /// <param name="albumId">Album ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>Paginated list of album images</returns>
    [HttpGet("album/{albumId}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ImageListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ImageListItemDto>>>> GetAlbumImages(
        string albumId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ListImagesQuery(pageNumber, pageSize);
        var result = await _uploadService.GetImagesByAlbumAsync(albumId, query);
        
        return Ok(result);
    }

    /// <summary>
    /// Delete an image permanently
    /// </summary>
    /// <param name="imageId">Image ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _uploadService.DeleteImageAsync(imageId, userId);
        
        if (!result.Success)
        {
            if (result.Message == "Unauthorized")
                return Forbid();
            return NotFound(result);
        }
        
        return NoContent();
    }

    /// <summary>
    /// Archive an image (soft delete)
    /// </summary>
    /// <param name="imageId">Image ID to archive</param>
    /// <returns>No content on success</returns>
    [HttpPost("{imageId}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveImage(Guid imageId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _uploadService.ArchiveImageAsync(imageId, userId);
        
        if (!result.Success)
        {
            if (result.Message == "Unauthorized")
                return Forbid();
            return NotFound(result);
        }
        
        return NoContent();
    }

    /// <summary>
    /// Get CDN URL for an image
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <returns>CDN URL</returns>
    [HttpGet("{imageId}/cdn-url")]
    [ProducesResponseType(typeof(ApiResponse<ImageCdnUrlDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ImageCdnUrlDto>>> GetCdnUrl(Guid imageId)
    {
        var image = await _uploadService.GetImageAsync(imageId);
        if (!image.Success)
            return NotFound(image);

        var cdnUrlDto = new ImageCdnUrlDto(
            imageId,
            image.Data!.CdnUrl ?? "/images/placeholder.png",
            DateTime.UtcNow.AddDays(30));

        return Ok(new ApiResponse<ImageCdnUrlDto>(true, cdnUrlDto, "CDN URL retrieved"));
    }

    /// <summary>
    /// Get transformed CDN URL with resize and format options
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <param name="request">Transform request parameters</param>
    /// <returns>Transformed CDN URL</returns>
    [HttpPost("{imageId}/cdn-url/transform")]
    [ProducesResponseType(typeof(ApiResponse<ImageCdnUrlDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ImageCdnUrlDto>>> GetTransformedCdnUrl(
        Guid imageId,
        [FromBody] GetImageWithTransformRequest request)
    {
        var image = await _uploadService.GetImageAsync(imageId);
        if (!image.Success)
            return NotFound(image);

        var transformedUrl = _cdnService.GetTransformedUrl(
            image.Data!.StoragePath,
            request.Width,
            request.Height,
            request.Format,
            request.Quality ?? 85);

        var cdnUrlDto = new ImageCdnUrlDto(
            imageId,
            transformedUrl,
            DateTime.UtcNow.AddDays(30));

        return Ok(new ApiResponse<ImageCdnUrlDto>(true, cdnUrlDto, "Transformed CDN URL generated"));
    }

    /// <summary>
    /// Update image metadata and description
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <param name="request">Update request</param>
    /// <returns>No content on success</returns>
    [HttpPut("{imageId}/metadata")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMetadata(Guid imageId, [FromBody] UpdateImageMetadataRequest request)
    {
        var result = await _metadataService.UpdateMetadataAsync(imageId, request);
        
        if (!result.Success)
            return NotFound(result);
        
        return NoContent();
    }

    /// <summary>
    /// Add tags to an image
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <param name="tags">List of tags to add</param>
    /// <returns>No content on success</returns>
    [HttpPost("{imageId}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTags(Guid imageId, [FromBody] List<string> tags)
    {
        var result = await _metadataService.AddTagsAsync(imageId, tags);
        
        if (!result.Success)
            return NotFound(result);
        
        return NoContent();
    }

    /// <summary>
    /// Remove a tag from an image
    /// </summary>
    /// <param name="imageId">Image ID</param>
    /// <param name="tag">Tag to remove</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{imageId}/tags/{tag}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTag(Guid imageId, string tag)
    {
        var result = await _metadataService.RemoveTagAsync(imageId, tag);
        
        if (!result.Success)
            return NotFound(result);
        
        return NoContent();
    }

    /// <summary>
    /// Health check endpoint (anonymous)
    /// </summary>
    /// <returns>Health status</returns>
    [AllowAnonymous]
    [HttpGet("health")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public ActionResult<string> Health()
    {
        return Ok("Healthy");
    }
}

/// <summary>
/// API Controller for image generation operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class GenerationController : ControllerBase
{
    private readonly IImageGenerationService _generationService;

    public GenerationController(IImageGenerationService generationService)
    {
        _generationService = generationService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Generate an image using AI
    /// </summary>
    /// <param name="request">Generation request with prompt</param>
    /// <returns>Generated image with generation status</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(ApiResponse<GenerationStatusDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<GenerationStatusDto>>> GenerateImage([FromBody] GenerateImageRequest request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _generationService.GenerateImageAsync(request, userId);
        
        if (!result.Success)
        {
            if (result.Message?.Contains("unavailable") == true)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
            return BadRequest(result);
        }
        
        return CreatedAtAction(nameof(GetGenerationStatus), new { generationId = result.Data?.GenerationId }, result);
    }

    /// <summary>
    /// Get the status of an image generation request
    /// </summary>
    /// <param name="generationId">Generation request ID</param>
    /// <returns>Generation status and progress</returns>
    [HttpGet("{generationId}/status")]
    [ProducesResponseType(typeof(ApiResponse<GenerationStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GenerationStatusDto>>> GetGenerationStatus(Guid generationId)
    {
        var result = await _generationService.GetGenerationStatusAsync(generationId);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }
}
