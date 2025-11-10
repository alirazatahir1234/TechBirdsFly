using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageService.Application.DTOs;
using ImageService.Application.Interfaces;
using ImageService.Domain.Entities;
using ImageService.Domain.Events;

namespace ImageService.Application.Services;

/// <summary>
/// Service for image upload operations
/// </summary>
public class ImageUploadService : IImageUploadService
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageMetadataRepository _metadataRepository;
    private readonly IImageStorageService _storageService;
    private readonly IEventPublisher _eventPublisher;

    public ImageUploadService(
        IImageRepository imageRepository,
        IImageMetadataRepository metadataRepository,
        IImageStorageService storageService,
        IEventPublisher eventPublisher)
    {
        _imageRepository = imageRepository;
        _metadataRepository = metadataRepository;
        _storageService = storageService;
        _eventPublisher = eventPublisher;
    }

    public async Task<ApiResponse<ImageDto>> UploadImageAsync(UploadImageRequest request, Guid userId)
    {
        try
        {
            // Parse format
            if (!Enum.TryParse<ImageFormat>(request.MimeType.Split('/')[1].ToUpper(), out var format))
                return new ApiResponse<ImageDto>(false, null, "Invalid image format", new List<string> { "Unsupported MIME type" });

            // Create resolution
            var resolution = new ImageResolution(request.Width, request.Height);
            if (!resolution.IsValid())
                return new ApiResponse<ImageDto>(false, null, "Invalid resolution", new List<string> { "Width and height must be greater than 0" });

            // Create file metadata
            var fileMetadata = new FileMetadata(request.FileName, request.MimeType, request.FileSizeBytes);
            if (!fileMetadata.IsValid())
                return new ApiResponse<ImageDto>(false, null, "Invalid file metadata", new List<string> { "File metadata validation failed" });

            // Generate storage path
            string storagePath = $"uploads/{userId:N}/{Guid.NewGuid():N}{Path.GetExtension(request.FileName)}";

            // Create image aggregate
            var imageResult = Image.CreateFromUpload(
                request.FileName,
                storagePath,
                format,
                resolution,
                fileMetadata,
                userId,
                request.AlbumId,
                request.Description);

            if (!imageResult.IsSuccess)
                return new ApiResponse<ImageDto>(false, null, imageResult.Error);

            var image = imageResult.Data!;

            // Add tags if provided
            if (request.Tags != null && request.Tags.Count > 0)
            {
                foreach (var tag in request.Tags)
                {
                    try { image.AddTag(tag); }
                    catch { /* Skip invalid tags */ }
                }
            }

            // Save to repository
            var savedImage = await _imageRepository.CreateAsync(image);

            // Create metadata
            var metadata = new ImageMetadata(savedImage.Id);
            await _metadataRepository.CreateAsync(metadata);

            // Publish event
            var uploadEvent = new ImageUploadedEvent
            {
                AggregateId = savedImage.Id,
                FileName = request.FileName,
                StoragePath = storagePath,
                FileSizeBytes = request.FileSizeBytes,
                UploadedByUserId = userId
            };
            await _eventPublisher.PublishEventAsync(uploadEvent);

            return new ApiResponse<ImageDto>(true, MapToDto(savedImage), "Image uploaded successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse<ImageDto>(false, null, "Upload failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ImageDto>> GetImageAsync(Guid imageId)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
                return new ApiResponse<ImageDto>(false, null, "Image not found");

            return new ApiResponse<ImageDto>(true, MapToDto(image), "Image retrieved successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse<ImageDto>(false, null, "Retrieval failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<PaginatedResponse<ImageListItemDto>>> ListUserImagesAsync(Guid userId, ListImagesQuery query)
    {
        try
        {
            int skip = (query.PageNumber - 1) * query.PageSize;
            var images = await _imageRepository.GetUserImagesAsync(userId, skip, query.PageSize);
            var total = await _imageRepository.GetUserImageCountAsync(userId);

            var items = images.Select(MapToListDto).ToList();
            int totalPages = (int)Math.Ceiling(total / (double)query.PageSize);

            var response = new PaginatedResponse<ImageListItemDto>(
                items, total, query.PageNumber, query.PageSize, totalPages);

            return new ApiResponse<PaginatedResponse<ImageListItemDto>>(true, response, "Images listed successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaginatedResponse<ImageListItemDto>>(false, null, "List failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<PaginatedResponse<ImageListItemDto>>> GetImagesByAlbumAsync(string albumId, ListImagesQuery query)
    {
        try
        {
            int skip = (query.PageNumber - 1) * query.PageSize;
            var images = await _imageRepository.GetImagesByAlbumAsync(albumId, skip, query.PageSize);

            var items = images.Select(MapToListDto).ToList();
            int totalPages = 1;

            var response = new PaginatedResponse<ImageListItemDto>(
                items, items.Count, query.PageNumber, query.PageSize, totalPages);

            return new ApiResponse<PaginatedResponse<ImageListItemDto>>(true, response, "Album images listed successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse<PaginatedResponse<ImageListItemDto>>(false, null, "List failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse> DeleteImageAsync(Guid imageId, Guid userId)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
                return new ApiResponse(false, "Image not found");

            if (image.CreatedByUserId != userId)
                return new ApiResponse(false, "Unauthorized");

            // Delete storage
            await _storageService.DeleteImageAsync(image.StoragePath);

            // Delete from repository
            await _imageRepository.DeleteAsync(imageId);

            // Publish event
            var deleteEvent = new ImageDeletedEvent
            {
                AggregateId = imageId,
                DeletedByUserId = userId
            };
            await _eventPublisher.PublishEventAsync(deleteEvent);

            return new ApiResponse(true, "Image deleted successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, "Delete failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse> ArchiveImageAsync(Guid imageId, Guid userId)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
                return new ApiResponse(false, "Image not found");

            if (image.CreatedByUserId != userId)
                return new ApiResponse(false, "Unauthorized");

            image.Archive();
            await _imageRepository.UpdateAsync(image);

            var archiveEvent = new ImageArchivedEvent
            {
                AggregateId = imageId,
                ArchivedByUserId = userId
            };
            await _eventPublisher.PublishEventAsync(archiveEvent);

            return new ApiResponse(true, "Image archived successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, "Archive failed", new List<string> { ex.Message });
        }
    }

    private ImageDto MapToDto(Image image) =>
        new(
            image.Id,
            image.FileName,
            image.StoragePath,
            image.Format.ToString(),
            image.Status.ToString(),
            image.Source.ToString(),
            image.Resolution.Width,
            image.Resolution.Height,
            image.FileMetadata.FileSizeBytes,
            image.Description,
            image.CdnUrl,
            image.ThumbnailUrl,
            image.OptimizedUrl,
            image.Tags.ToList(),
            image.CreatedByUserId,
            image.AlbumId,
            image.CreatedAt,
            image.ModifiedAt,
            image.ArchivedAt);

    private ImageListItemDto MapToListDto(Image image) =>
        new(
            image.Id,
            image.FileName,
            image.Format.ToString(),
            image.Status.ToString(),
            image.ThumbnailUrl,
            image.Resolution.Width,
            image.Resolution.Height,
            image.CreatedAt);
}

/// <summary>
/// Service for image generation operations
/// </summary>
public class ImageGenerationService : IImageGenerationService
{
    private readonly IAIImageGenerationService _aiService;
    private readonly IImageRepository _imageRepository;
    private readonly IEventPublisher _eventPublisher;

    public ImageGenerationService(
        IAIImageGenerationService aiService,
        IImageRepository imageRepository,
        IEventPublisher eventPublisher)
    {
        _aiService = aiService;
        _imageRepository = imageRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ApiResponse<GenerationStatusDto>> GenerateImageAsync(GenerateImageRequest request, Guid userId)
    {
        try
        {
            if (!await _aiService.IsAvailableAsync())
                return new ApiResponse<GenerationStatusDto>(false, null, "AI service unavailable");

            var generationId = Guid.NewGuid();
            var imageUrl = await _aiService.GenerateImageAsync(request.Prompt, request.Model ?? "dall-e-3", request.Width ?? 1024, request.Height ?? 1024);

            // Create image from generated URL
            var fileName = $"generated-{generationId}.png";
            var resolution = new ImageResolution(request.Width ?? 1024, request.Height ?? 1024);
            var fileMetadata = new FileMetadata(fileName, "image/png", 0);

            var imageResult = Image.CreateAIGenerated(fileName, imageUrl, ImageFormat.PNG, resolution, fileMetadata, userId, request.Prompt);
            if (!imageResult.IsSuccess)
                return new ApiResponse<GenerationStatusDto>(false, null, imageResult.Error);

            var image = imageResult.Data!;
            var savedImage = await _imageRepository.CreateAsync(image);

            // Publish event
            var genEvent = new ImageGeneratedEvent
            {
                AggregateId = savedImage.Id,
                FileName = fileName,
                Prompt = request.Prompt,
                GeneratedByUserId = userId
            };
            await _eventPublisher.PublishEventAsync(genEvent);

            var status = new GenerationStatusDto(generationId, savedImage.Id, "Completed", 100, null, DateTime.UtcNow, DateTime.UtcNow);
            return new ApiResponse<GenerationStatusDto>(true, status, "Image generated successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse<GenerationStatusDto>(false, null, "Generation failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<GenerationStatusDto>> GetGenerationStatusAsync(Guid generationId)
    {
        try
        {
            // In real implementation, query from database
            var status = new GenerationStatusDto(generationId, Guid.Empty, "Pending", 0);
            return new ApiResponse<GenerationStatusDto>(true, status, "Status retrieved");
        }
        catch (Exception ex)
        {
            return new ApiResponse<GenerationStatusDto>(false, null, "Retrieval failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse> UpdateGenerationStatusAsync(Guid generationId, string status, string? errorMessage = null)
    {
        try
        {
            // In real implementation, update database
            return new ApiResponse(true, "Status updated successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, "Update failed", new List<string> { ex.Message });
        }
    }
}

/// <summary>
/// Service for image metadata operations
/// </summary>
public class ImageMetadataService : IImageMetadataService
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageMetadataRepository _metadataRepository;
    private readonly IEventPublisher _eventPublisher;

    public ImageMetadataService(
        IImageRepository imageRepository,
        IImageMetadataRepository metadataRepository,
        IEventPublisher eventPublisher)
    {
        _imageRepository = imageRepository;
        _metadataRepository = metadataRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ApiResponse<ImageMetadataDto>> GetMetadataAsync(Guid imageId)
    {
        try
        {
            var metadata = await _metadataRepository.GetByImageIdAsync(imageId);
            if (metadata == null)
                return new ApiResponse<ImageMetadataDto>(false, null, "Metadata not found");

            return new ApiResponse<ImageMetadataDto>(true, MapToDto(metadata), "Metadata retrieved");
        }
        catch (Exception ex)
        {
            return new ApiResponse<ImageMetadataDto>(false, null, "Retrieval failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ImageMetadataDto>> CreateMetadataAsync(Guid imageId)
    {
        try
        {
            var metadata = new ImageMetadata(imageId);
            var saved = await _metadataRepository.CreateAsync(metadata);
            return new ApiResponse<ImageMetadataDto>(true, MapToDto(saved), "Metadata created");
        }
        catch (Exception ex)
        {
            return new ApiResponse<ImageMetadataDto>(false, null, "Creation failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse<ImageMetadataDto>> UpdateMetadataAsync(Guid imageId, UpdateImageMetadataRequest request)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
                return new ApiResponse<ImageMetadataDto>(false, null, "Image not found");

            if (request.Description != null)
                image.Description = request.Description;

            if (request.Tags != null)
            {
                // Clear and add new tags
                foreach (var tag in image.Tags.ToList())
                    image.RemoveTag(tag);

                foreach (var tag in request.Tags)
                {
                    try { image.AddTag(tag); }
                    catch { /* Skip invalid tags */ }
                }
            }

            await _imageRepository.UpdateAsync(image);

            var metadata = await _metadataRepository.GetByImageIdAsync(imageId) ?? new ImageMetadata(imageId);

            if (request.ExifData != null)
                metadata.UpdateExifData(request.ExifData);

            if (request.CustomProperties != null)
            {
                foreach (var prop in request.CustomProperties)
                    metadata.SetCustomProperty(prop.Key, prop.Value);
            }

            var saved = await _metadataRepository.UpdateAsync(metadata);

            var updateEvent = new ImageMetadataUpdatedEvent
            {
                AggregateId = imageId,
                MetadataType = "Full",
                NewValue = System.Text.Json.JsonSerializer.Serialize(request)
            };
            await _eventPublisher.PublishEventAsync(updateEvent);

            return new ApiResponse<ImageMetadataDto>(true, MapToDto(saved), "Metadata updated");
        }
        catch (Exception ex)
        {
            return new ApiResponse<ImageMetadataDto>(false, null, "Update failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse> AddTagsAsync(Guid imageId, List<string> tags)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
                return new ApiResponse(false, "Image not found");

            foreach (var tag in tags)
            {
                try { image.AddTag(tag); }
                catch { /* Skip duplicates */ }
            }

            await _imageRepository.UpdateAsync(image);
            return new ApiResponse(true, "Tags added successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, "Add tags failed", new List<string> { ex.Message });
        }
    }

    public async Task<ApiResponse> RemoveTagAsync(Guid imageId, string tag)
    {
        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
                return new ApiResponse(false, "Image not found");

            image.RemoveTag(tag);
            await _imageRepository.UpdateAsync(image);
            return new ApiResponse(true, "Tag removed successfully");
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, "Remove tag failed", new List<string> { ex.Message });
        }
    }

    private ImageMetadataDto MapToDto(ImageMetadata metadata) =>
        new(
            metadata.Id,
            metadata.ImageId,
            metadata.ExifData,
            metadata.ColorProfile,
            metadata.DPI,
            metadata.HasTransparency,
            metadata.AnimationFrameCount,
            metadata.CustomProperties,
            metadata.CreatedAt,
            metadata.ModifiedAt);
}
