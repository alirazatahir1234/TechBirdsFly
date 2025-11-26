using TechBirdsFly.MediaService.Domain.Entities;

namespace TechBirdsFly.MediaService.Domain.Interfaces;

public interface IMediaRepository
{
    Task<MediaFile?> GetByIdAsync(Guid id);
    Task<List<MediaFile>> GetAllAsync();
    Task AddAsync(MediaFile file);
    Task DeleteAsync(MediaFile file);
    Task SaveChangesAsync();
}
