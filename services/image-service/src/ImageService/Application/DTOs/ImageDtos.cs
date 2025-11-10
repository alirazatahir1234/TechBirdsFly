using System;
using System.Collections.Generic;
using ImageService.Domain.Entities;

namespace ImageService.Application.DTOs;

#region Request DTOs

/// <summary>
/// Request to upload an image
/// </summary>
public record UploadImageRequest(
    string FileName,
    string MimeType,
    long FileSizeBytes,
    int Width,
    int Height,
    string? AlbumId = null,
    string? Description = null,
    List<string>? Tags = null);

/// <summary>
/// Request to generate image with AI
/// </summary>
public record GenerateImageRequest(
    string Prompt,
    string? Model = "dall-e-3",
    int? Width = 1024,
    int? Height = 1024,
    int? NumberOfImages = 1,
    string? Style = null);

/// <summary>
/// Request to update image metadata
/// </summary>
public record UpdateImageMetadataRequest(
    string? Description = null,
    List<string>? Tags = null,
    string? ExifData = null,
    Dictionary<string, string>? CustomProperties = null);

/// <summary>
/// Request to get CDN URL with transformations
/// </summary>
public record GetImageWithTransformRequest(
    int? Width = null,
    int? Height = null,
    string? Format = null,
    int? Quality = 85,
    bool? AutoOptimize = true);

/// <summary>
/// Request to bulk tag images
/// </summary>
public record BulkTagImagesRequest(
    List<Guid> ImageIds,
    List<string> Tags,
    bool RemoveExistingTags = false);

/// <summary>
/// Request to bulk archive images
/// </summary>
public record BulkArchiveImagesRequest(
    List<Guid> ImageIds,
    string? Reason = null);

#endregion

#region Response DTOs

/// <summary>
/// Image DTO for API responses
/// </summary>
public record ImageDto(
    Guid Id,
    string FileName,
    string StoragePath,
    string Format,
    string Status,
    string Source,
    int Width,
    int Height,
    long FileSizeBytes,
    string? Description,
    string? CdnUrl,
    string? ThumbnailUrl,
    string? OptimizedUrl,
    List<string> Tags,
    Guid CreatedByUserId,
    string? AlbumId,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    DateTime? ArchivedAt);

/// <summary>
/// Simplified image DTO for list responses
/// </summary>
public record ImageListItemDto(
    Guid Id,
    string FileName,
    string Format,
    string Status,
    string? ThumbnailUrl,
    int Width,
    int Height,
    DateTime CreatedAt);

/// <summary>
/// Image metadata DTO
/// </summary>
public record ImageMetadataDto(
    Guid Id,
    Guid ImageId,
    string? ExifData,
    string? ColorProfile,
    int? DPI,
    bool HasTransparency,
    string? AnimationFrameCount,
    Dictionary<string, string> CustomProperties,
    DateTime CreatedAt,
    DateTime? ModifiedAt);

/// <summary>
/// CDN URL response DTO
/// </summary>
public record ImageCdnUrlDto(
    Guid ImageId,
    string Url,
    DateTime ExpiresAt,
    string? SignedUrl = null);

/// <summary>
/// Image statistics DTO
/// </summary>
public record UserImageStatisticsDto(
    Guid UserId,
    int TotalImages,
    int AvailableImages,
    int ProcessingImages,
    int ArchivedImages,
    long TotalStorageUsedBytes,
    DateTime? LastUploadDate);

/// <summary>
/// Image format statistics DTO
/// </summary>
public record ImageFormatStatisticsDto(
    string Format,
    int Count,
    long TotalSizeBytes,
    double AverageWidthPixels,
    double AverageHeightPixels);

/// <summary>
/// List query for pagination
/// </summary>
public record ListImagesQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool Ascending = true,
    string? FilterByStatus = null,
    string? FilterBySource = null,
    string? FilterByFormat = null);

/// <summary>
/// Paginated response wrapper
/// </summary>
public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages)
{
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Generic API response
/// </summary>
public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null,
    List<string>? Errors = null);

/// <summary>
/// Error response
/// </summary>
public record ApiResponse(
    bool Success,
    string? Message = null,
    List<string>? Errors = null);

/// <summary>
/// Generation status DTO
/// </summary>
public record GenerationStatusDto(
    Guid GenerationId,
    Guid ImageId,
    string Status,
    int ProgressPercentage,
    string? ErrorMessage = null,
    DateTime StartedAt = default,
    DateTime? CompletedAt = null);

#endregion
