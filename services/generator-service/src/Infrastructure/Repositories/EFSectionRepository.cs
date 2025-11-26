using Microsoft.EntityFrameworkCore;
using GeneratorService.Domain.Entities;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Domain.ValueObjects;
using GeneratorService.Infrastructure.Persistence;

namespace GeneratorService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISectionRepository
/// Provides CRUD operations for Section entities
/// </summary>
public class EFSectionRepository : ISectionRepository
{
    private readonly GeneratorDbContext _dbContext;

    public EFSectionRepository(GeneratorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sections
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Section>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sections
            .Where(s => s.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Section>> GetByTypeAsync(Guid projectId, SectionType type, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sections
            .Where(s => s.ProjectId == projectId && s.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Section section, CancellationToken cancellationToken = default)
    {
        await _dbContext.Sections.AddAsync(section, cancellationToken);
    }

    public async Task UpdateAsync(Section section, CancellationToken cancellationToken = default)
    {
        _dbContext.Sections.Update(section);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var section = await GetByIdAsync(sectionId, cancellationToken);
        if (section != null)
        {
            _dbContext.Sections.Remove(section);
        }
    }

    public async Task DeleteByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var sections = await GetByProjectIdAsync(projectId, cancellationToken);
        _dbContext.Sections.RemoveRange(sections);
        await Task.CompletedTask;
    }
}
