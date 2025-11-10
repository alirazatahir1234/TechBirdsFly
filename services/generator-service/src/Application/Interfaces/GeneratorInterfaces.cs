namespace GeneratorService.Application.Interfaces;

using GeneratorService.Domain.Entities;
using GeneratorService.Application.DTOs;

/// <summary>
/// Repository interfaces for data access
/// </summary>

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Template>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Template>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Template>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Template>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default);
    Task<Template> UpdateAsync(Template template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IGenerationRepository
{
    Task<Generation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Generation>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Generation>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Generation> CreateAsync(Generation generation, CancellationToken cancellationToken = default);
    Task<Generation> UpdateAsync(Generation generation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service interfaces
/// </summary>

public interface ITemplateApplicationService
{
    Task<TemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TemplateDto>> GetTemplatesByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<TemplateDto>> GetTemplatesByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<TemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default);
    Task<TemplateDto> UpdateTemplateAsync(Guid id, UpdateTemplateRequest request, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IProjectApplicationService
{
    Task<ProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectDto> PublishProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectDto> ArchiveProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IGenerationApplicationService
{
    Task<GenerationDto?> GetGenerationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GenerationDto>> GetProjectGenerationsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<GenerationDto> GenerateProjectAsync(GenerateProjectRequest request, CancellationToken cancellationToken = default);
    Task<GenerationDto> RetryGenerationAsync(Guid generationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// External service interfaces
/// </summary>

public interface IAIGeneratorService
{
    Task<string> GenerateWebsiteAsync(string prompt, string templateContent, string configuration, CancellationToken cancellationToken = default);
    Task<decimal> EstimateCreditsAsync(string prompt, CancellationToken cancellationToken = default);
}

public interface IStorageService
{
    Task<string> SaveGeneratedContentAsync(string projectId, string content, CancellationToken cancellationToken = default);
    Task<string?> RetrieveContentAsync(string projectId, string generationId, CancellationToken cancellationToken = default);
    Task DeleteContentAsync(string projectId, CancellationToken cancellationToken = default);
}

public interface IEventPublisher
{
    Task PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
