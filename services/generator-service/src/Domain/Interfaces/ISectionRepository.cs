using GeneratorService.Domain.Entities;

namespace GeneratorService.Domain.Interfaces;

/// <summary>
/// Repository interface for Section entity
/// Defines contracts for persisting and retrieving sections
/// </summary>
public interface ISectionRepository
{
    /// <summary>
    /// Gets a section by its ID
    /// </summary>
    Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sections for a project
    /// </summary>
    Task<IEnumerable<Section>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sections by type for a project
    /// </summary>
    Task<IEnumerable<Section>> GetByTypeAsync(Guid projectId, Domain.ValueObjects.SectionType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new section
    /// </summary>
    Task AddAsync(Section section, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing section
    /// </summary>
    Task UpdateAsync(Section section, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a section
    /// </summary>
    Task DeleteAsync(Guid sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all sections for a project
    /// </summary>
    Task DeleteByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}
