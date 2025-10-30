using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ImageService.Services;
using ImageService.Services.Cache;

namespace ImageService.Controllers;

[ApiController]
[Route("api/image")]
[Authorize]
public class ImageController : ControllerBase
{
    private readonly IImageGenerationService _generationService;
    private readonly IImageStorageService _storageService;
    private readonly ICacheService _cache;
    private readonly ILogger<ImageController> _logger;

    public ImageController(
        IImageGenerationService generationService,
        IImageStorageService storageService,
        ICacheService cache,
        ILogger<ImageController> logger)
    {
        _generationService = generationService;
        _storageService = storageService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "image-service",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Generate image from text prompt using OpenAI DALL-E 3
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateImage([FromBody] GenerateImageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { error = "Prompt is required" });
            }

            _logger.LogInformation("Generating image from prompt: {Prompt}", request.Prompt);

            var startTime = DateTime.UtcNow;
            var result = await _generationService.GenerateImageAsync(
                request.Prompt,
                request.Size ?? "1024x1024",
                request.Style,
                request.Count ?? 1);

            if (!result.Success)
            {
                return StatusCode(500, new { error = result.ErrorMessage ?? "Failed to generate image" });
            }

            var generationTime = (DateTime.UtcNow - startTime).TotalSeconds;

            _logger.LogInformation("Image generated successfully. Time: {Time}s, Cost: ${Cost}",
                generationTime, result.Cost);

            return Ok(new
            {
                status = "success",
                imageUrl = result.ImageUrl,
                thumbnailUrl = result.ThumbnailUrl,
                generationTime = generationTime,
                cost = result.Cost,
                timestamp = DateTime.UtcNow,
                imageId = result.ImageId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image");
            return StatusCode(500, new { error = "Failed to generate image", details = ex.Message });
        }
    }

    /// <summary>
    /// Upload image file
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string? description = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file provided" });
            }

            // Validate file type
            var validTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!validTypes.Contains(file.ContentType))
            {
                return BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, WebP, GIF" });
            }

            // Validate file size (50MB max)
            if (file.Length > 52428800)
            {
                return BadRequest(new { error = "File too large. Maximum 50MB allowed" });
            }

            _logger.LogInformation("Uploading image: {FileName}, Size: {Size}bytes",
                file.FileName, file.Length);

            using var stream = file.OpenReadStream();
            var result = await _storageService.UploadImageAsync(
                stream,
                file.FileName,
                file.ContentType,
                description);

            if (!result.Success)
            {
                return StatusCode(500, new { error = result.ErrorMessage ?? "Failed to upload image" });
            }

            return Ok(new
            {
                status = "success",
                imageUrl = result.ImageUrl,
                thumbnailUrl = result.ThumbnailUrl,
                size = file.Length,
                format = Path.GetExtension(file.FileName),
                timestamp = DateTime.UtcNow,
                imageId = result.ImageId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { error = "Failed to upload image", details = ex.Message });
        }
    }

    /// <summary>
    /// Get image by ID
    /// </summary>
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetImage(string imageId)
    {
        try
        {
            var cacheKey = $"image:{imageId}";
            var cachedImage = await _cache.GetAsync<dynamic>(cacheKey);
            if (cachedImage != null)
                return Ok(cachedImage);

            var image = await _storageService.GetImageAsync(imageId);
            if (image == null)
            {
                return NotFound(new { error = "Image not found" });
            }

            var result = new
            {
                id = image.Id,
                url = image.ImageUrl,
                thumbnailUrl = image.ThumbnailUrl,
                createdAt = image.CreatedAt,
                description = image.Description,
                format = image.Format,
                size = image.Size
            };

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving image");
            return StatusCode(500, new { error = "Failed to retrieve image" });
        }
    }

    /// <summary>
    /// List user's images
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> ListImages([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            var cacheKey = $"image-list:{userId}:{limit}:{offset}";
            var cachedImages = await _cache.GetAsync<dynamic>(cacheKey);
            if (cachedImages != null)
                return Ok(cachedImages);

            var images = await _storageService.ListImagesAsync(userId, limit, offset);

            var result = new
            {
                total = images.Count,
                limit,
                offset,
                images = images.Select(img => new
                {
                    id = img.Id,
                    url = img.ImageUrl,
                    thumbnailUrl = img.ThumbnailUrl,
                    createdAt = img.CreatedAt,
                    description = img.Description
                })
            };

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing images");
            return StatusCode(500, new { error = "Failed to list images" });
        }
    }

    /// <summary>
    /// Delete image
    /// </summary>
    [HttpDelete("{imageId}")]
    public async Task<IActionResult> DeleteImage(string imageId)
    {
        try
        {
            var success = await _storageService.DeleteImageAsync(imageId);
            if (!success)
            {
                return NotFound(new { error = "Image not found" });
            }

            // Invalidate cache
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            await _cache.RemoveAsync($"image:{imageId}");
            // Clear user's image list cache (using wildcard pattern would be ideal, but we clear known keys)
            for (int i = 0; i < 5; i++) // Clear first 5 page combinations
            {
                await _cache.RemoveAsync($"image-list:{userId}:20:{i * 20}");
                await _cache.RemoveAsync($"image-list:{userId}:50:{i * 50}");
            }

            _logger.LogInformation("Image deleted: {ImageId}", imageId);
            return Ok(new { status = "deleted", message = "Image removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image");
            return StatusCode(500, new { error = "Failed to delete image" });
        }
    }

    /// <summary>
    /// Get generation statistics
    /// </summary>
    [HttpGet("stats/summary")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var cacheKey = "image-stats:summary";
            var cachedStats = await _cache.GetAsync<dynamic>(cacheKey);
            if (cachedStats != null)
                return Ok(cachedStats);

            var stats = await _generationService.GetStatisticsAsync();

            var result = new
            {
                timestamp = DateTime.UtcNow,
                imagesGenerated = stats.ImagesGenerated,
                totalCost = stats.TotalCost,
                averageGenerationTime = stats.AverageGenerationTime,
                failedGenerations = stats.FailedGenerations,
                storageUsed = stats.TotalStorageUsed,
                successRate = stats.SuccessRate
            };

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return StatusCode(500, new { error = "Failed to get statistics" });
        }
    }
}

// ============================================================================
// REQUEST DTOs
// ============================================================================

public class GenerateImageRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Style { get; set; }
    public int? Count { get; set; }
}
