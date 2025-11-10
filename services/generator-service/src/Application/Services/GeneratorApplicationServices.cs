namespace GeneratorService.Application.Services;

using Microsoft.Extensions.Logging;
using GeneratorService.Domain.Entities;
using GeneratorService.Domain.Events;
using GeneratorService.Application.DTOs;
using GeneratorService.Application.Interfaces;

/// <summary>
/// Application service for template operations
/// </summary>
public class TemplateApplicationService : ITemplateApplicationService
{
    private readonly ITemplateRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<TemplateApplicationService> _logger;

    public TemplateApplicationService(
        ITemplateRepository repository,
        IEventPublisher eventPublisher,
        ILogger<TemplateApplicationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TemplateDto?> GetTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting template with ID: {TemplateId}", id);
        var template = await _repository.GetByIdAsync(id, cancellationToken);
        return template == null ? null : MapToDto(template);
    }

    public async Task<IEnumerable<TemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all templates");
        var templates = await _repository.GetAllAsync(cancellationToken);
        return templates.Select(MapToDto);
    }

    public async Task<IEnumerable<TemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting active templates");
        var templates = await _repository.GetActiveAsync(cancellationToken);
        return templates.Select(MapToDto);
    }

    public async Task<IEnumerable<TemplateDto>> GetTemplatesByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting templates by type: {Type}", type);
        var templates = await _repository.GetByTypeAsync(type, cancellationToken);
        return templates.Select(MapToDto);
    }

    public async Task<IEnumerable<TemplateDto>> GetTemplatesByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting templates by category: {Category}", category);
        var templates = await _repository.GetByCategoryAsync(category, cancellationToken);
        return templates.Select(MapToDto);
    }

    public async Task<TemplateDto> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating template: {TemplateName}", request.Name);

        var template = Template.Create(
            request.Name,
            request.Description,
            Enum.Parse<TemplateType>(request.Type),
            request.Category,
            request.Content,
            request.ThumbnailUrl);

        var created = await _repository.CreateAsync(template, cancellationToken);

        var @event = new TemplateCreatedEvent(
            created.Id,
            created.Name,
            created.Category,
            created.Type.ToString());

        await _eventPublisher.PublishEventAsync(@event, cancellationToken);

        _logger.LogInformation("Template created successfully with ID: {TemplateId}", created.Id);
        return MapToDto(created);
    }

    public async Task<TemplateDto> UpdateTemplateAsync(Guid id, UpdateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating template: {TemplateId}", id);

        var template = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Template with ID {id} not found");

        if (!string.IsNullOrEmpty(request.Name))
            template.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Description))
            template.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Content))
            template.Content = request.Content;
        if (!string.IsNullOrEmpty(request.ThumbnailUrl))
            template.ThumbnailUrl = request.ThumbnailUrl;
        template.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(template, cancellationToken);
        _logger.LogInformation("Template updated successfully: {TemplateId}", id);
        return MapToDto(updated);
    }

    public async Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting template: {TemplateId}", id);
        var template = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Template with ID {id} not found");

        template.Deactivate();
        await _repository.UpdateAsync(template, cancellationToken);
        _logger.LogInformation("Template deactivated: {TemplateId}", id);
    }

    private static TemplateDto MapToDto(Template template) =>
        new(
            template.Id,
            template.Name,
            template.Description,
            template.Type.ToString(),
            template.Category,
            template.ThumbnailUrl,
            template.IsActive,
            template.UseCount,
            template.CreatedAt,
            template.UpdatedAt);
}

/// <summary>
/// Application service for project operations
/// </summary>
public class ProjectApplicationService : IProjectApplicationService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProjectApplicationService> _logger;

    public ProjectApplicationService(
        IProjectRepository projectRepository,
        ITemplateRepository templateRepository,
        IEventPublisher eventPublisher,
        ILogger<ProjectApplicationService> logger)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting project: {ProjectId}", id);
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        return project == null ? null : MapToDto(project);
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all projects");
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return projects.Select(MapToDto);
    }

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting projects for user: {UserId}", userId);
        var projects = await _projectRepository.GetByUserIdAsync(userId, cancellationToken);
        return projects.Select(MapToDto);
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating project: {ProjectName} for user: {UserId}", request.Name, request.UserId);

        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template with ID {request.TemplateId} not found");

        var project = Project.Create(
            request.UserId,
            request.Name,
            request.Description,
            request.TemplateId,
            request.Configuration ?? "{}");

        var created = await _projectRepository.CreateAsync(project, cancellationToken);

        var @event = new ProjectCreatedEvent(
            created.Id,
            created.UserId,
            created.Name,
            created.TemplateId);

        await _eventPublisher.PublishEventAsync(@event, cancellationToken);

        _logger.LogInformation("Project created successfully: {ProjectId}", created.Id);
        return MapToDto(created);
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating project: {ProjectId}", id);

        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Project with ID {id} not found");

        project.Name = request.Name ?? project.Name;
        project.Description = request.Description ?? project.Description;
        if (!string.IsNullOrEmpty(request.Configuration))
            project.Configuration = request.Configuration;
        project.UpdatedAt = DateTime.UtcNow;

        var updated = await _projectRepository.UpdateAsync(project, cancellationToken);
        _logger.LogInformation("Project updated successfully: {ProjectId}", id);
        return MapToDto(updated);
    }

    public async Task<ProjectDto> PublishProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing project: {ProjectId}", id);

        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Project with ID {id} not found");

        if (project.Status != ProjectStatus.Generated)
            throw new InvalidOperationException($"Project must be in Generated state to publish. Current state: {project.Status}");

        project.Publish();
        var updated = await _projectRepository.UpdateAsync(project, cancellationToken);

        var @event = new ProjectPublishedEvent(
            updated.Id,
            updated.UserId,
            updated.Name,
            updated.OutputUrl ?? string.Empty);

        await _eventPublisher.PublishEventAsync(@event, cancellationToken);

        _logger.LogInformation("Project published successfully: {ProjectId}", id);
        return MapToDto(updated);
    }

    public async Task<ProjectDto> ArchiveProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Archiving project: {ProjectId}", id);

        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Project with ID {id} not found");

        project.Archive();
        var updated = await _projectRepository.UpdateAsync(project, cancellationToken);
        _logger.LogInformation("Project archived successfully: {ProjectId}", id);
        return MapToDto(updated);
    }

    public async Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting project: {ProjectId}", id);
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Project with ID {id} not found");

        await _projectRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Project deleted successfully: {ProjectId}", id);
    }

    private static ProjectDto MapToDto(Project project) =>
        new(
            project.Id,
            project.UserId,
            project.Name,
            project.Description,
            project.TemplateId,
            project.Status.ToString(),
            project.OutputUrl,
            project.GenerationCount,
            project.CreatedAt,
            project.UpdatedAt,
            project.PublishedAt);
}

/// <summary>
/// Application service for generation operations
/// </summary>
public class GenerationApplicationService : IGenerationApplicationService
{
    private readonly IGenerationRepository _repository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAIGeneratorService _aiService;
    private readonly IStorageService _storageService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<GenerationApplicationService> _logger;

    public GenerationApplicationService(
        IGenerationRepository repository,
        IProjectRepository projectRepository,
        IAIGeneratorService aiService,
        IStorageService storageService,
        IEventPublisher eventPublisher,
        ILogger<GenerationApplicationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GenerationDto?> GetGenerationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting generation: {GenerationId}", id);
        var generation = await _repository.GetByIdAsync(id, cancellationToken);
        return generation == null ? null : MapToDto(generation);
    }

    public async Task<IEnumerable<GenerationDto>> GetProjectGenerationsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting generations for project: {ProjectId}", projectId);
        var generations = await _repository.GetByProjectIdAsync(projectId, cancellationToken);
        return generations.Select(MapToDto);
    }

    public async Task<GenerationDto> GenerateProjectAsync(GenerateProjectRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting generation for project: {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new InvalidOperationException($"Project with ID {request.ProjectId} not found");

        // Estimate credits
        var estimatedCredits = await _aiService.EstimateCreditsAsync(request.AiPrompt, cancellationToken);

        // Create generation
        var generation = Generation.Create(
            request.ProjectId,
            project.TemplateId,
            request.CustomConfiguration ?? "{}",
            estimatedCredits);

        var created = await _repository.CreateAsync(generation, cancellationToken);

        // Update project status
        project.GenerationCount++;
        await _projectRepository.UpdateAsync(project, cancellationToken);

        // Publish event
        var @event = new GenerationStartedEvent(
            created.Id,
            created.ProjectId,
            created.TemplateId,
            estimatedCredits);

        await _eventPublisher.PublishEventAsync(@event, cancellationToken);

        _logger.LogInformation("Generation started successfully: {GenerationId}", created.Id);
        return MapToDto(created);
    }

    public async Task<GenerationDto> RetryGenerationAsync(Guid generationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrying generation: {GenerationId}", generationId);

        var generation = await _repository.GetByIdAsync(generationId, cancellationToken)
            ?? throw new InvalidOperationException($"Generation with ID {generationId} not found");

        if (generation.Status != GenerationStatus.Failed)
            throw new InvalidOperationException($"Can only retry failed generations. Current status: {generation.Status}");

        generation.MarkAsInProgress();
        var updated = await _repository.UpdateAsync(generation, cancellationToken);

        _logger.LogInformation("Generation retry initiated: {GenerationId}", generationId);
        return MapToDto(updated);
    }

    private static GenerationDto MapToDto(Generation generation) =>
        new(
            generation.Id,
            generation.ProjectId,
            generation.TemplateId,
            generation.Status.ToString(),
            generation.OutputPath,
            generation.ErrorMessage,
            generation.EstimatedCreditsUsed,
            generation.StartedAt,
            generation.CompletedAt);
}
