namespace GeneratorService.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern for managing transactions
/// Coordinates changes across multiple repositories
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets the project repository
    /// </summary>
    IProjectRepository Projects { get; }

    /// <summary>
    /// Gets the section repository
    /// </summary>
    ISectionRepository Sections { get; }

    /// <summary>
    /// Gets the generated page repository
    /// </summary>
    IGeneratedPageRepository GeneratedPages { get; }

    /// <summary>
    /// Saves all changes made in this unit of work
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
