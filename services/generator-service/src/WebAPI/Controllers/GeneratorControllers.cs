namespace GeneratorService.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using GeneratorService.Application.DTOs;
using GeneratorService.Application.Interfaces;

/// <summary>
/// API controller for template management operations
/// Provides endpoints for CRUD operations on templates
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateApplicationService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(
        ITemplateApplicationService templateService,
        ILogger<TemplatesController> logger)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all available templates
    /// </summary>
    /// <response code="200">Returns list of all templates</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TemplateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTemplates(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all templates");
        var templates = await _templateService.GetAllTemplatesAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<TemplateDto>>.SuccessResponse(templates, "Templates retrieved successfully"));
    }

    /// <summary>
    /// Get active templates only
    /// </summary>
    /// <response code="200">Returns list of active templates</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TemplateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveTemplates(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/templates/active - Retrieving active templates");
        var templates = await _templateService.GetActiveTemplatesAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<TemplateDto>>.SuccessResponse(templates, "Active templates retrieved successfully"));
    }

    /// <summary>
    /// Get templates by type
    /// </summary>
    /// <param name="type">Template type (Blog, Portfolio, ECommerce, etc.)</param>
    /// <response code="200">Returns list of templates matching the type</response>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TemplateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplatesByType(string type, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/templates/type/{Type} - Retrieving templates by type: {Type}", type);
        var templates = await _templateService.GetTemplatesByTypeAsync(type, cancellationToken);
        return Ok(ApiResponse<IEnumerable<TemplateDto>>.SuccessResponse(templates, $"Templates of type '{type}' retrieved successfully"));
    }

    /// <summary>
    /// Get templates by category
    /// </summary>
    /// <param name="category">Template category</param>
    /// <response code="200">Returns list of templates matching the category</response>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TemplateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplatesByCategory(string category, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/templates/category/{Category} - Retrieving templates by category: {Category}", category);
        var templates = await _templateService.GetTemplatesByCategoryAsync(category, cancellationToken);
        return Ok(ApiResponse<IEnumerable<TemplateDto>>.SuccessResponse(templates, $"Templates in category '{category}' retrieved successfully"));
    }

    /// <summary>
    /// Get a specific template by ID
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <response code="200">Returns the template</response>
    /// <response code="404">Template not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/templates/{TemplateId} - Retrieving template: {TemplateId}", id);
        var template = await _templateService.GetTemplateAsync(id, cancellationToken);

        if (template == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Template with ID '{id}' not found"));

        return Ok(ApiResponse<TemplateDto>.SuccessResponse(template, "Template retrieved successfully"));
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    /// <param name="request">Template creation request</param>
    /// <response code="201">Template created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TemplateDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate(CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/templates - Creating new template: {TemplateName}", request.Name);

        try
        {
            var template = await _templateService.CreateTemplateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, 
                ApiResponse<TemplateDto>.SuccessResponse(template, "Template created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update an existing template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="request">Template update request</param>
    /// <response code="200">Template updated successfully</response>
    /// <response code="404">Template not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(Guid id, UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/templates/{TemplateId} - Updating template: {TemplateId}", id);

        try
        {
            var template = await _templateService.UpdateTemplateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<TemplateDto>.SuccessResponse(template, "Template updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Deactivate a template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <response code="204">Template deactivated successfully</response>
    /// <response code="404">Template not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DELETE /api/templates/{TemplateId} - Deactivating template: {TemplateId}", id);

        try
        {
            await _templateService.DeleteTemplateAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

/// <summary>
/// API controller for project management operations
/// Provides endpoints for CRUD and workflow operations on projects
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectApplicationService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IProjectApplicationService projectService,
        ILogger<ProjectsController> logger)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all projects
    /// </summary>
    /// <response code="200">Returns list of all projects</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProjects(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/projects - Retrieving all projects");
        var projects = await _projectService.GetAllProjectsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<ProjectDto>>.SuccessResponse(projects, "Projects retrieved successfully"));
    }

    /// <summary>
    /// Get projects for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <response code="200">Returns list of user's projects</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserProjects(Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/projects/user/{UserId} - Retrieving projects for user: {UserId}", userId);
        var projects = await _projectService.GetUserProjectsAsync(userId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<ProjectDto>>.SuccessResponse(projects, "User projects retrieved successfully"));
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <response code="200">Returns the project</response>
    /// <response code="404">Project not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProject(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/projects/{ProjectId} - Retrieving project: {ProjectId}", id);
        var project = await _projectService.GetProjectAsync(id, cancellationToken);

        if (project == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Project with ID '{id}' not found"));

        return Ok(ApiResponse<ProjectDto>.SuccessResponse(project, "Project retrieved successfully"));
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="request">Project creation request</param>
    /// <response code="201">Project created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/projects - Creating new project: {ProjectName} for user: {UserId}", request.Name, request.UserId);

        try
        {
            var project = await _projectService.CreateProjectAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id },
                ApiResponse<ProjectDto>.SuccessResponse(project, "Project created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="request">Project update request</param>
    /// <response code="200">Project updated successfully</response>
    /// <response code="404">Project not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT /api/projects/{ProjectId} - Updating project: {ProjectId}", id);

        try
        {
            var project = await _projectService.UpdateProjectAsync(id, request, cancellationToken);
            return Ok(ApiResponse<ProjectDto>.SuccessResponse(project, "Project updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Publish a project (makes it public)
    /// Project must be in Generated state
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <response code="200">Project published successfully</response>
    /// <response code="404">Project not found</response>
    /// <response code="400">Project cannot be published in current state</response>
    [HttpPost("{id}/publish")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishProject(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/projects/{ProjectId}/publish - Publishing project: {ProjectId}", id);

        try
        {
            var project = await _projectService.PublishProjectAsync(id, cancellationToken);
            return Ok(ApiResponse<ProjectDto>.SuccessResponse(project, "Project published successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found")
                ? NotFound(ApiResponse<object>.ErrorResponse(ex.Message)) as IActionResult
                : BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Archive a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <response code="200">Project archived successfully</response>
    /// <response code="404">Project not found</response>
    [HttpPost("{id}/archive")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveProject(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/projects/{ProjectId}/archive - Archiving project: {ProjectId}", id);

        try
        {
            var project = await _projectService.ArchiveProjectAsync(id, cancellationToken);
            return Ok(ApiResponse<ProjectDto>.SuccessResponse(project, "Project archived successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <response code="204">Project deleted successfully</response>
    /// <response code="404">Project not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DELETE /api/projects/{ProjectId} - Deleting project: {ProjectId}", id);

        try
        {
            await _projectService.DeleteProjectAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

/// <summary>
/// API controller for generation operations
/// Provides endpoints for initiating and monitoring website generation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GenerationsController : ControllerBase
{
    private readonly IGenerationApplicationService _generationService;
    private readonly ILogger<GenerationsController> _logger;

    public GenerationsController(
        IGenerationApplicationService generationService,
        ILogger<GenerationsController> logger)
    {
        _generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all generations for a specific project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <response code="200">Returns list of generations for the project</response>
    [HttpGet("project/{projectId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GenerationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectGenerations(Guid projectId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/generations/project/{ProjectId} - Retrieving generations for project: {ProjectId}", projectId);
        var generations = await _generationService.GetProjectGenerationsAsync(projectId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<GenerationDto>>.SuccessResponse(generations, "Generations retrieved successfully"));
    }

    /// <summary>
    /// Get a specific generation by ID
    /// </summary>
    /// <param name="id">Generation ID</param>
    /// <response code="200">Returns the generation</response>
    /// <response code="404">Generation not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GenerationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGeneration(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/generations/{GenerationId} - Retrieving generation: {GenerationId}", id);
        var generation = await _generationService.GetGenerationAsync(id, cancellationToken);

        if (generation == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Generation with ID '{id}' not found"));

        return Ok(ApiResponse<GenerationDto>.SuccessResponse(generation, "Generation retrieved successfully"));
    }

    /// <summary>
    /// Start generating a project
    /// Creates a new generation and initiates the AI-powered website generation
    /// </summary>
    /// <param name="request">Generation request with project ID and AI prompt</param>
    /// <response code="201">Generation started successfully</response>
    /// <response code="400">Invalid request or insufficient credits</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(ApiResponse<GenerationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateProject(GenerateProjectRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/generations/generate - Starting generation for project: {ProjectId}", request.ProjectId);

        try
        {
            var generation = await _generationService.GenerateProjectAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetGeneration), new { id = generation.Id },
                ApiResponse<GenerationDto>.SuccessResponse(generation, "Generation started successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Retry a failed generation
    /// </summary>
    /// <param name="generationId">Generation ID</param>
    /// <response code="200">Generation retry initiated</response>
    /// <response code="404">Generation not found</response>
    /// <response code="400">Generation is not in failed state</response>
    [HttpPost("{generationId}/retry")]
    [ProducesResponseType(typeof(ApiResponse<GenerationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RetryGeneration(Guid generationId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/generations/{GenerationId}/retry - Retrying generation: {GenerationId}", generationId);

        try
        {
            var generation = await _generationService.RetryGenerationAsync(generationId, cancellationToken);
            return Ok(ApiResponse<GenerationDto>.SuccessResponse(generation, "Generation retry initiated"));
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found")
                ? NotFound(ApiResponse<object>.ErrorResponse(ex.Message)) as IActionResult
                : BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
