using Microsoft.EntityFrameworkCore;
using GeneratorService.Domain.Entities;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Infrastructure.Persistence;

namespace GeneratorService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IGeneratedPageRepository
/// Provides CRUD operations for GeneratedPage entities
/// </summary>
public class EFGeneratedPageRepository : IGeneratedPageRepository
{
    private readonly GeneratorDbContext _dbContext;

    public EFGeneratedPageRepository(GeneratorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GeneratedPage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.GeneratedPages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<GeneratedPage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.GeneratedPages
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GeneratedPage>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.GeneratedPages
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(GeneratedPage page, CancellationToken cancellationToken = default)
    {
        await _dbContext.GeneratedPages.AddAsync(page, cancellationToken);
    }

    public async Task UpdateAsync(GeneratedPage page, CancellationToken cancellationToken = default)
    {
        _dbContext.GeneratedPages.Update(page);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var page = await GetByIdAsync(pageId, cancellationToken);
        if (page != null)
        {
            _dbContext.GeneratedPages.Remove(page);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
