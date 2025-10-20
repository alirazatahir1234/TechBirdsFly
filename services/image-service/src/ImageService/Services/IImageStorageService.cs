using ImageService.Models;
using ImageService.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Services;

/// <summary>
/// Service for storing and managing images
/// </summary>
public interface IImageStorageService
{
    Task<ImageStorageResult> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? description = null);

    Task<Image?> GetImageAsync(string imageId);
    Task<List<Image>> ListImagesAsync(string userId, int limit = 20, int offset = 0);
    Task<bool> DeleteImageAsync(string imageId);
}

/// <summary>
/// Implementation of image storage service with local file storage
/// </summary>
public class ImageStorageService : IImageStorageService
{
    private readonly ILogger<ImageStorageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ImageDbContext _context;

    public ImageStorageService(
        ILogger<ImageStorageService> logger,
        IConfiguration configuration,
        ImageDbContext context)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
    }

    public async Task<ImageStorageResult> UploadImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? description = null)
    {
        try
        {
            var imageId = Guid.NewGuid().ToString();
            var storageType = _configuration["Storage__Type"] ?? "Local";

            if (storageType == "Local")
            {
                return await UploadToLocalStorageAsync(fileStream, imageId, fileName, description);
            }
            else if (storageType == "Cloudinary")
            {
                return await UploadToCloudinaryAsync(fileStream, imageId, fileName, description);
            }

            throw new InvalidOperationException($"Unknown storage type: {storageType}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image: {FileName}", fileName);
            return new ImageStorageResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<Image?> GetImageAsync(string imageId)
    {
        try
        {
            var image = await _context.Images.FirstOrDefaultAsync(i =>
                i.Id == imageId && !i.IsDeleted);
            return image;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image: {ImageId}", imageId);
            return null;
        }
    }

    public async Task<List<Image>> ListImagesAsync(string userId, int limit = 20, int offset = 0)
    {
        try
        {
            var images = await _context.Images
                .Where(i => i.UserId == userId && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return images;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing images for user: {UserId}", userId);
            return new List<Image>();
        }
    }

    public async Task<bool> DeleteImageAsync(string imageId)
    {
        try
        {
            var image = await _context.Images.FirstOrDefaultAsync(i => i.Id == imageId);
            if (image == null)
            {
                return false;
            }

            image.IsDeleted = true;
            image.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image marked as deleted: {ImageId}", imageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImageId}", imageId);
            return false;
        }
    }

    private async Task<ImageStorageResult> UploadToLocalStorageAsync(
        Stream fileStream,
        string imageId,
        string fileName,
        string? description)
    {
        try
        {
            var storagePath = _configuration["Storage__LocalPath"] ?? "/app/storage";
            var directory = Path.Combine(storagePath, "images");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var extension = Path.GetExtension(fileName);
            var filename = $"{imageId}{extension}";
            var filepath = Path.Combine(directory, filename);

            using (var file = File.Create(filepath))
            {
                await fileStream.CopyToAsync(file);
            }

            var fileInfo = new FileInfo(filepath);

            var image = new Image
            {
                Id = imageId,
                UserId = "local-user", // Should come from auth context
                ImageUrl = $"/storage/images/{filename}",
                ThumbnailUrl = $"/storage/thumbnails/{imageId}_thumb{extension}",
                Description = description,
                Format = extension.TrimStart('.'),
                Size = fileInfo.Length,
                Source = "uploaded"
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image uploaded successfully: {ImageId}, Size: {Size}",
                imageId, fileInfo.Length);

            return new ImageStorageResult
            {
                ImageId = imageId,
                ImageUrl = image.ImageUrl,
                ThumbnailUrl = image.ThumbnailUrl,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image to local storage: {ImageId}", imageId);
            throw;
        }
    }

    private async Task<ImageStorageResult> UploadToCloudinaryAsync(
        Stream fileStream,
        string imageId,
        string fileName,
        string? description)
    {
        // TODO: Implement Cloudinary upload
        _logger.LogWarning("Cloudinary upload not yet implemented");

        var image = new Image
        {
            Id = imageId,
            UserId = "local-user",
            ImageUrl = $"https://cloudinary.example.com/images/{imageId}",
            ThumbnailUrl = $"https://cloudinary.example.com/thumbs/{imageId}_thumb",
            Description = description,
            Format = Path.GetExtension(fileName).TrimStart('.'),
            Size = 0,
            Source = "uploaded"
        };

        _context.Images.Add(image);
        await _context.SaveChangesAsync();

        return new ImageStorageResult
        {
            ImageId = imageId,
            ImageUrl = image.ImageUrl,
            ThumbnailUrl = image.ThumbnailUrl,
            Success = true
        };
    }
}
