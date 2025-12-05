using PublishService.Domain.Entities;

namespace PublishService.Domain.Interfaces;

/// <summary>
/// Repository for publish operations
/// </summary>
public interface IPublishRepository
{
    Task AddAsync(PublishRecord record);
    Task<PublishRecord?> GetByIdAsync(Guid id);
    Task<PublishRecord?> GetLatestByProjectAsync(Guid projectId);
    Task<List<PublishRecord>> GetByProjectAsync(Guid projectId, int limit = 20);
    Task UpdateAsync(PublishRecord record);
    Task SaveChangesAsync();
}
