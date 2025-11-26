using TechBirdsFly.EditorService.Domain.Entities;

namespace TechBirdsFly.EditorService.Domain.Interfaces;

/// <summary>
/// Repository for managing Section entities
/// </summary>
public interface ISectionRepository
{
    /// <summary>
    /// Get a section by its ID
    /// </summary>
    Task<Section?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all sections for a project, ordered by their display order
    /// </summary>
    Task<List<Section>> GetByProjectIdAsync(Guid projectId);

    /// <summary>
    /// Add a new section
    /// </summary>
    Task AddAsync(Section section);

    /// <summary>
    /// Delete a section
    /// </summary>
    Task DeleteAsync(Section section);

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    Task SaveChangesAsync();
}
