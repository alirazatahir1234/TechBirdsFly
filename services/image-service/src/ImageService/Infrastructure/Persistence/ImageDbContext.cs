using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ImageService.Application.Interfaces;
using ImageService.Domain.Entities;

namespace ImageService.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for Image Service
/// </summary>
public class ImageDbContext : DbContext
{
    public DbSet<Image> Images { get; set; }
    public DbSet<ImageMetadata> ImageMetadata { get; set; }

    public ImageDbContext(DbContextOptions<ImageDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Image aggregate
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CdnUrl).HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.OptimizedUrl).HasMaxLength(500);
            entity.Property(e => e.AlbumId).HasMaxLength(100);

            // Value Objects
            entity.OwnsOne(e => e.Resolution, resolution =>
            {
                resolution.Property(r => r.Width).HasColumnName("Resolution_Width");
                resolution.Property(r => r.Height).HasColumnName("Resolution_Height");
            });

            entity.OwnsOne(e => e.FileMetadata, metadata =>
            {
                metadata.Property(m => m.FileName).HasColumnName("FileMetadata_FileName").IsRequired();
                metadata.Property(m => m.MimeType).HasColumnName("FileMetadata_MimeType").IsRequired();
                metadata.Property(m => m.FileSizeBytes).HasColumnName("FileMetadata_FileSizeBytes");
            });

            // Relationships
            entity.HasMany<ImageMetadata>()
                .WithOne()
                .HasForeignKey(m => m.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indices for performance
            entity.HasIndex(e => e.CreatedByUserId).HasName("IX_Images_CreatedByUserId");
            entity.HasIndex(e => e.Status).HasName("IX_Images_Status");
            entity.HasIndex(e => e.AlbumId).HasName("IX_Images_AlbumId");
            entity.HasIndex(e => e.CreatedAt).HasName("IX_Images_CreatedAt");
            entity.HasIndex(e => new { e.CreatedByUserId, e.Status }).HasName("IX_Images_UserId_Status");
            entity.HasIndex(e => new { e.AlbumId, e.Status }).HasName("IX_Images_AlbumId_Status");
        });

        // Configure ImageMetadata
        modelBuilder.Entity<ImageMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ImageId).IsRequired();
            entity.Property(e => e.ExifData).HasMaxLength(5000);
            entity.Property(e => e.ColorProfile).HasMaxLength(255);
            entity.Property(e => e.AnimationFrameCount).HasMaxLength(10);

            entity.HasIndex(e => e.ImageId).HasName("IX_ImageMetadata_ImageId");
            entity.HasIndex(e => e.CreatedAt).HasName("IX_ImageMetadata_CreatedAt");
        });
    }
}

/// <summary>
/// Repository for Image aggregate
/// </summary>
public class ImageRepository : IImageRepository
{
    private readonly ImageDbContext _context;

    public ImageRepository(ImageDbContext context)
    {
        _context = context;
    }

    public async Task<Image?> GetByIdAsync(Guid id)
    {
        return await _context.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Image?> GetByStoragePathAsync(string storagePath)
    {
        return await _context.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.StoragePath == storagePath);
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _context.Images.AnyAsync(i => i.Id == id);
    }

    public async Task<List<Image>> GetUserImagesAsync(Guid userId, int skip = 0, int take = 20)
    {
        return await _context.Images
            .Where(i => i.CreatedByUserId == userId && i.Status != ImageStatus.Archived)
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Image>> GetImagesByAlbumAsync(string albumId, int skip = 0, int take = 20)
    {
        return await _context.Images
            .Where(i => i.AlbumId == albumId && i.Status != ImageStatus.Archived)
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Image>> GetImagesByStatusAsync(ImageStatus status, int skip = 0, int take = 20)
    {
        return await _context.Images
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetUserImageCountAsync(Guid userId)
    {
        return await _context.Images
            .Where(i => i.CreatedByUserId == userId && i.Status != ImageStatus.Archived)
            .CountAsync();
    }

    public async Task<Image> CreateAsync(Image image)
    {
        _context.Images.Add(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task<Image> UpdateAsync(Image image)
    {
        _context.Images.Update(image);
        await _context.SaveChangesAsync();
        return image;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var image = await _context.Images.FindAsync(id);
        if (image == null)
            return false;

        _context.Images.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }
}

/// <summary>
/// Repository for ImageMetadata
/// </summary>
public class ImageMetadataRepository : IImageMetadataRepository
{
    private readonly ImageDbContext _context;

    public ImageMetadataRepository(ImageDbContext context)
    {
        _context = context;
    }

    public async Task<ImageMetadata?> GetByIdAsync(Guid id)
    {
        return await _context.ImageMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<ImageMetadata?> GetByImageIdAsync(Guid imageId)
    {
        return await _context.ImageMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ImageId == imageId);
    }

    public async Task<List<ImageMetadata>> GetByImageIdsAsync(List<Guid> imageIds)
    {
        return await _context.ImageMetadata
            .Where(m => imageIds.Contains(m.ImageId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ImageMetadata> CreateAsync(ImageMetadata metadata)
    {
        _context.ImageMetadata.Add(metadata);
        await _context.SaveChangesAsync();
        return metadata;
    }

    public async Task<ImageMetadata> UpdateAsync(ImageMetadata metadata)
    {
        _context.ImageMetadata.Update(metadata);
        await _context.SaveChangesAsync();
        return metadata;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var metadata = await _context.ImageMetadata.FindAsync(id);
        if (metadata == null)
            return false;

        _context.ImageMetadata.Remove(metadata);
        await _context.SaveChangesAsync();
        return true;
    }
}
