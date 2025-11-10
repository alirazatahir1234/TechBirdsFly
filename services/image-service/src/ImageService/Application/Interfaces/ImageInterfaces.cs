using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageService.Application.DTOs;
using ImageService.Domain.Entities;

namespace ImageService.Application.Interfaces;

#region Repository Interfaces

/// <summary>
/// Repository interface for Image aggregate
/// </summary>
public interface IImageRepository
{
    Task<Image?> GetByIdAsync(Guid id);
    Task<Image?> GetByStoragePathAsync(string storagePath);
    Task<bool> ExistsByIdAsync(Guid id);
    Task<List<Image>> GetUserImagesAsync(Guid userId, int skip = 0, int take = 20);
    Task<List<Image>> GetImagesByAlbumAsync(string albumId, int skip = 0, int take = 20);
    Task<List<Image>> GetImagesByStatusAsync(ImageStatus status, int skip = 0, int take = 20);
    Task<int> GetUserImageCountAsync(Guid userId);
    Task<Image> CreateAsync(Image image);
    Task<Image> UpdateAsync(Image image);
    Task<bool> DeleteAsync(Guid id);
}

/// <summary>
/// Repository interface for ImageMetadata
/// </summary>
public interface IImageMetadataRepository
{
    Task<ImageMetadata?> GetByIdAsync(Guid id);
    Task<ImageMetadata?> GetByImageIdAsync(Guid imageId);
    Task<List<ImageMetadata>> GetByImageIdsAsync(List<Guid> imageIds);
    Task<ImageMetadata> CreateAsync(ImageMetadata metadata);
    Task<ImageMetadata> UpdateAsync(ImageMetadata metadata);
    Task<bool> DeleteAsync(Guid id);
}

#endregion

#region Application Service Interfaces

/// <summary>
/// Service interface for image upload operations
/// </summary>
public interface IImageUploadService
{
    Task<ApiResponse<ImageDto>> UploadImageAsync(UploadImageRequest request, Guid userId);
    Task<ApiResponse<ImageDto>> GetImageAsync(Guid imageId);
    Task<ApiResponse<PaginatedResponse<ImageListItemDto>>> ListUserImagesAsync(Guid userId, ListImagesQuery query);
    Task<ApiResponse<PaginatedResponse<ImageListItemDto>>> GetImagesByAlbumAsync(string albumId, ListImagesQuery query);
    Task<ApiResponse> DeleteImageAsync(Guid imageId, Guid userId);
    Task<ApiResponse> ArchiveImageAsync(Guid imageId, Guid userId);
}

/// <summary>
/// Service interface for image generation
/// </summary>
public interface IImageGenerationService
{
    Task<ApiResponse<GenerationStatusDto>> GenerateImageAsync(GenerateImageRequest request, Guid userId);
    Task<ApiResponse<GenerationStatusDto>> GetGenerationStatusAsync(Guid generationId);
    Task<ApiResponse> UpdateGenerationStatusAsync(Guid generationId, string status, string? errorMessage = null);
}

/// <summary>
/// Service interface for image metadata operations
/// </summary>
public interface IImageMetadataService
{
    Task<ApiResponse<ImageMetadataDto>> GetMetadataAsync(Guid imageId);
    Task<ApiResponse<ImageMetadataDto>> CreateMetadataAsync(Guid imageId);
    Task<ApiResponse<ImageMetadataDto>> UpdateMetadataAsync(Guid imageId, UpdateImageMetadataRequest request);
    Task<ApiResponse> AddTagsAsync(Guid imageId, List<string> tags);
    Task<ApiResponse> RemoveTagAsync(Guid imageId, string tag);
}

/// <summary>
/// Service interface for CDN operations
/// </summary>
public interface IImageCdnService
{
    Task<string> GetCdnUrlAsync(Guid imageId);
    Task<string> GetTransformedUrlAsync(Guid imageId, GetImageWithTransformRequest request);
    Task<string> GenerateSignedUrlAsync(Guid imageId, TimeSpan expiresIn);
    Task<bool> InvalidateCdnCacheAsync(Guid imageId);
}

/// <summary>
/// Service interface for image queries
/// </summary>
public interface IImageQueryService
{
    Task<UserImageStatisticsDto> GetUserStatisticsAsync(Guid userId);
    Task<List<ImageFormatStatisticsDto>> GetFormatStatisticsAsync();
    Task<List<ImageListItemDto>> SearchImagesAsync(string searchTerm, Guid userId);
}

/// <summary>
/// Service interface for batch operations
/// </summary>
public interface IBatchImageOperationService
{
    Task<ApiResponse> BulkTagImagesAsync(BulkTagImagesRequest request, Guid userId);
    Task<ApiResponse> BulkArchiveImagesAsync(BulkArchiveImagesRequest request, Guid userId);
    Task<ApiResponse> BulkDeleteImagesAsync(List<Guid> imageIds, Guid userId);
}

#endregion

#region External Service Interfaces

/// <summary>
/// Service interface for image storage operations
/// </summary>
public interface IImageStorageService
{
    Task<string> UploadImageAsync(string fileName, byte[] fileData);
    Task<byte[]> DownloadImageAsync(string storagePath);
    Task<bool> DeleteImageAsync(string storagePath);
    Task<bool> ExistsAsync(string storagePath);
    Task<long> GetFileSizeAsync(string storagePath);
    Task<string> CreateThumbnailAsync(string storagePath, int width, int height);
}

/// <summary>
/// Service interface for image processing operations
/// </summary>
public interface IImageProcessingService
{
    Task<Dictionary<string, string>> AnalyzeImageAsync(byte[] imageData);
    Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height);
    Task<byte[]> OptimizeImageAsync(byte[] imageData, int quality = 85);
    Task<byte[]> ConvertFormatAsync(byte[] imageData, string targetFormat);
    Task<string> CalculateChecksumAsync(byte[] imageData);
}

/// <summary>
/// Service interface for AI image generation
/// </summary>
public interface IAIImageGenerationService
{
    Task<string> GenerateImageAsync(string prompt, string model = "dall-e-3", int width = 1024, int height = 1024);
    Task<bool> IsAvailableAsync();
    Task<int> GetMaxRequestsPerMinuteAsync();
}

/// <summary>
/// Service interface for CDN operations
/// </summary>
public interface ICdnService
{
    string GetCdnUrl(string storagePath);
    string GetTransformedUrl(string storagePath, int? width = null, int? height = null, string? format = null, int quality = 85);
    string GenerateSignedUrl(string storagePath, TimeSpan expiresIn);
    Task<bool> InvalidateCacheAsync(string storagePath);
}

/// <summary>
/// Service interface for event publishing
/// </summary>
public interface IEventPublisher
{
    Task PublishEventAsync<T>(T domainEvent) where T : class;
    Task PublishEventsAsync<T>(List<T> domainEvents) where T : class;
}

#endregion
