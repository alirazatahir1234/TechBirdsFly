using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ImageService.Domain.Entities;

/// <summary>
/// Image format enumeration
/// </summary>
public enum ImageFormat
{
    JPEG,
    PNG,
    WebP,
    GIF,
    SVG,
    AVIF,
    BMP,
    TIFF
}

/// <summary>
/// Image status enumeration
/// </summary>
public enum ImageStatus
{
    Pending,
    Processing,
    Available,
    Archived,
    Failed
}

/// <summary>
/// Image source enumeration
/// </summary>
public enum ImageSource
{
    UserUpload,
    AIGenerated,
    Template,
    Stock
}

/// <summary>
/// Image resolution value object
/// </summary>
public record ImageResolution(int Width, int Height)
{
    public ImageResolution() : this(0, 0) { }

    public bool IsValid() => Width > 0 && Height > 0;
    public int GetAspectRatio() => Width / Height;
    public long GetPixelCount() => (long)Width * Height;
}

/// <summary>
/// File metadata value object
/// </summary>
public record FileMetadata(string FileName, string MimeType, long FileSizeBytes)
{
    public FileMetadata() : this(string.Empty, string.Empty, 0) { }

    public bool IsValid() => !string.IsNullOrWhiteSpace(FileName) &&
                             !string.IsNullOrWhiteSpace(MimeType) &&
                             FileSizeBytes > 0;

    public string GetFileExtension() => Path.GetExtension(FileName);
    public decimal GetFileSizeMB() => FileSizeBytes / (1024m * 1024m);
}

/// <summary>
/// Result helper for domain operations
/// </summary>
public record Result<T>(bool IsSuccess, T? Data, string? Error)
{
    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

/// <summary>
/// Image aggregate root
/// </summary>
public class Image
{
    private readonly List<string> _tags = new();
    private readonly List<ImageMetadata> _metadata = new();

    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public ImageFormat Format { get; private set; }
    public ImageStatus Status { get; private set; }
    public ImageSource Source { get; private set; }
    public ImageResolution Resolution { get; private set; } = new();
    public FileMetadata FileMetadata { get; private set; } = new();
    public Guid CreatedByUserId { get; private set; }
    public string? AlbumId { get; private set; }
    public string? Description { get; set; }
    public string? CdnUrl { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? OptimizedUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public IReadOnlyList<ImageMetadata> Metadata => _metadata.AsReadOnly();

    public Image() { }

    /// <summary>
    /// Create image from user upload
    /// </summary>
    public static Result<Image> CreateFromUpload(
        string fileName,
        string storagePath,
        ImageFormat format,
        ImageResolution resolution,
        FileMetadata fileMetadata,
        Guid userId,
        string? albumId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result<Image>.Failure("FileName cannot be empty");

        if (string.IsNullOrWhiteSpace(storagePath))
            return Result<Image>.Failure("StoragePath cannot be empty");

        if (!resolution.IsValid())
            return Result<Image>.Failure("Resolution must have valid width and height");

        if (!fileMetadata.IsValid())
            return Result<Image>.Failure("FileMetadata is invalid");

        var image = new Image
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            StoragePath = storagePath,
            Format = format,
            Status = ImageStatus.Available,
            Source = ImageSource.UserUpload,
            Resolution = resolution,
            FileMetadata = fileMetadata,
            CreatedByUserId = userId,
            AlbumId = albumId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        return Result<Image>.Success(image);
    }

    /// <summary>
    /// Create image from AI generation
    /// </summary>
    public static Result<Image> CreateAIGenerated(
        string fileName,
        string storagePath,
        ImageFormat format,
        ImageResolution resolution,
        FileMetadata fileMetadata,
        Guid userId,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result<Image>.Failure("FileName cannot be empty");

        var image = new Image
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            StoragePath = storagePath,
            Format = format,
            Status = ImageStatus.Pending,
            Source = ImageSource.AIGenerated,
            Resolution = resolution,
            FileMetadata = fileMetadata,
            CreatedByUserId = userId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        return Result<Image>.Success(image);
    }

    /// <summary>
    /// Mark image as processing
    /// </summary>
    public void MarkProcessing()
    {
        if (Status != ImageStatus.Pending)
            throw new InvalidOperationException($"Cannot mark image as processing when status is {Status}");

        Status = ImageStatus.Processing;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark image as available
    /// </summary>
    public void MarkAvailable(string? cdnUrl = null, string? thumbnailUrl = null, string? optimizedUrl = null)
    {
        if (Status == ImageStatus.Archived)
            throw new InvalidOperationException("Cannot mark archived image as available");

        Status = ImageStatus.Available;
        CdnUrl = cdnUrl;
        ThumbnailUrl = thumbnailUrl;
        OptimizedUrl = optimizedUrl;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark image as failed
    /// </summary>
    public void MarkFailed()
    {
        Status = ImageStatus.Failed;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Archive image (soft delete)
    /// </summary>
    public void Archive()
    {
        if (Status == ImageStatus.Archived)
            throw new InvalidOperationException("Image is already archived");

        Status = ImageStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add tag to image
    /// </summary>
    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty");

        if (_tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Tag '{tag}' already exists");

        _tags.Add(tag.ToLowerInvariant());
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove tag from image
    /// </summary>
    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty");

        _tags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update resolution
    /// </summary>
    public void UpdateResolution(ImageResolution resolution)
    {
        if (!resolution.IsValid())
            throw new ArgumentException("Resolution must be valid");

        Resolution = resolution;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update CDN information
    /// </summary>
    public void UpdateCdnInfo(string cdnUrl, string? thumbnailUrl = null, string? optimizedUrl = null)
    {
        if (string.IsNullOrWhiteSpace(cdnUrl))
            throw new ArgumentException("CDN URL cannot be empty");

        CdnUrl = cdnUrl;
        ThumbnailUrl = thumbnailUrl;
        OptimizedUrl = optimizedUrl;
        ModifiedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Image metadata entity
/// </summary>
public class ImageMetadata
{
    public Guid Id { get; private set; }
    public Guid ImageId { get; private set; }
    public string? ExifData { get; set; }
    public string? ColorProfile { get; set; }
    public int? DPI { get; set; }
    public bool HasTransparency { get; set; }
    public string? AnimationFrameCount { get; set; }
    [NotMapped]
    public Dictionary<string, string> CustomProperties { get; set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    public ImageMetadata() { }

    public ImageMetadata(Guid imageId)
    {
        Id = Guid.NewGuid();
        ImageId = imageId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateExifData(string? exifData)
    {
        ExifData = exifData;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetCustomProperty(string key, string value)
    {
        CustomProperties[key] = value;
        ModifiedAt = DateTime.UtcNow;
    }

    public string? GetCustomProperty(string key)
    {
        return CustomProperties.TryGetValue(key, out var value) ? value : null;
    }
}
