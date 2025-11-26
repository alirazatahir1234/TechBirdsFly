using GeneratorService.Domain.Entities;

namespace GeneratorService.Domain.Interfaces;

/// <summary>
/// Repository interface for GeneratedPage entity
/// </summary>
public interface IGeneratedPageRepository
{
    /// <summary>
    /// Gets a page by its ID
    /// </summary>
    Task<GeneratedPage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pages
    /// </summary>
    Task<IEnumerable<GeneratedPage>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets published pages
    /// </summary>
    Task<IEnumerable<GeneratedPage>> GetPublishedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new page
    /// </summary>
    Task AddAsync(GeneratedPage page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing page
    /// </summary>
    Task UpdateAsync(GeneratedPage page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a page
    /// </summary>
    Task DeleteAsync(Guid pageId, CancellationToken cancellationToken = default);
}
