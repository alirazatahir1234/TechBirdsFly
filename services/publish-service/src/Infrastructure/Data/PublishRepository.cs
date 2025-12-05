using PublishService.Domain.Entities;
using PublishService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PublishService.Infrastructure.Data;

/// <summary>
/// Repository implementation for PublishService
/// </summary>
public class PublishRepository : IPublishRepository
{
    private readonly PublishDbContext _context;

    public PublishRepository(PublishDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PublishRecord record)
    {
        await _context.PublishRecords.AddAsync(record);
    }

    public async Task<PublishRecord?> GetByIdAsync(Guid id)
    {
        return await _context.PublishRecords.FindAsync(id);
    }

    public async Task<PublishRecord?> GetLatestByProjectAsync(Guid projectId)
    {
        return await _context.PublishRecords
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PublishRecord>> GetByProjectAsync(Guid projectId, int limit = 20)
    {
        return await _context.PublishRecords
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task UpdateAsync(PublishRecord record)
    {
        _context.PublishRecords.Update(record);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
