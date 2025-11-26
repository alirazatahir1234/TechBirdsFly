using GeneratorService.Domain.Entities;

namespace GeneratorService.Domain.Interfaces;

/// <summary>
/// Repository interface for Project entity
/// Defines contracts for persisting and retrieving projects
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Gets a project by its ID
    /// </summary>
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projects
    /// </summary>
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets projects by industry
    /// </summary>
    Task<IEnumerable<Project>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new project
    /// </summary>
    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing project
    /// </summary>
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project
    /// </summary>
    Task DeleteAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a project exists
    /// </summary>
    Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
}
