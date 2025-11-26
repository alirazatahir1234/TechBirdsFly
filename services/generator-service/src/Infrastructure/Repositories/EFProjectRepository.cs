using Microsoft.EntityFrameworkCore;
using GeneratorService.Domain.Entities;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Infrastructure.Persistence;

namespace GeneratorService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IProjectRepository
/// Provides CRUD operations for Project aggregate root
/// </summary>
public class EFProjectRepository : IProjectRepository
{
    private readonly GeneratorDbContext _dbContext;

    public EFProjectRepository(GeneratorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Include(p => p.Sections)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Include(p => p.Sections)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .Include(p => p.Sections)
            .Where(p => p.Industry == industry)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Update(project);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await GetByIdAsync(projectId, cancellationToken);
        if (project != null)
        {
            _dbContext.Projects.Remove(project);
        }
    }

    public async Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AnyAsync(p => p.Id == projectId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
