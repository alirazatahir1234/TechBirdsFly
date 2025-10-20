namespace ImageService.Models;

/// <summary>
/// Represents a generated or uploaded image
/// </summary>
public class Image
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? OriginalPrompt { get; set; }
    public string? Description { get; set; }
    public string Format { get; set; } = "png";
    public long Size { get; set; }
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    public string Source { get; set; } = "generated"; // generated or uploaded
    public decimal? GenerationCost { get; set; }
    public double? GenerationTimeSeconds { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Generation statistics
/// </summary>
public class GenerationStatistics
{
    public int ImagesGenerated { get; set; }
    public int ImagesUploaded { get; set; }
    public int FailedGenerations { get; set; }
    public decimal TotalCost { get; set; }
    public double AverageGenerationTime { get; set; }
    public long TotalStorageUsed { get; set; }
    public double SuccessRate { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Image generation result
/// </summary>
public class ImageGenerationResult
{
    public string ImageId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public double GenerationTime { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Image storage result
/// </summary>
public class ImageStorageResult
{
    public string ImageId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
