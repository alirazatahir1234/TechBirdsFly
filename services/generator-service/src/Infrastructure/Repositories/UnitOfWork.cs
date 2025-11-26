using GeneratorService.Domain.Interfaces;
using GeneratorService.Infrastructure.Persistence;

namespace GeneratorService.Infrastructure.Repositories;

/// <summary>
/// Unit of Work pattern implementation for transaction management
/// Coordinates multiple repositories and database operations
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GeneratorDbContext _dbContext;
    private IProjectRepository? _projectRepository;
    private ISectionRepository? _sectionRepository;
    private IGeneratedPageRepository? _generatedPageRepository;

    public UnitOfWork(GeneratorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IProjectRepository Projects =>
        _projectRepository ??= new EFProjectRepository(_dbContext);

    public ISectionRepository Sections =>
        _sectionRepository ??= new EFSectionRepository(_dbContext);

    public IGeneratedPageRepository GeneratedPages =>
        _generatedPageRepository ??= new EFGeneratedPageRepository(_dbContext);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
        }
        catch
        {
            // Already rolled back or no active transaction
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
    }
}
