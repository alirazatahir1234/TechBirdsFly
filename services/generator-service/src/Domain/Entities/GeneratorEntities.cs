namespace GeneratorService.Domain.Entities;

/// <summary>
/// Template aggregate root - represents a website template
/// </summary>
public sealed class Template
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TemplateType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;  // HTML/CSS template content
    public string ThumbnailUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int UseCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Template() { }

    /// <summary>
    /// Factory method to create a new template
    /// </summary>
    public static Template Create(string name, string description, TemplateType type, string category, string content, string? thumbnailUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Template content is required", nameof(content));

        return new Template
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Category = category,
            Content = content,
            ThumbnailUrl = thumbnailUrl ?? string.Empty,
            IsActive = true,
            UseCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Increment template usage count
    /// </summary>
    public void IncrementUseCount()
    {
        UseCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate template
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate template
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Project aggregate root - represents a generated website project
/// </summary>
public sealed class Project
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public ProjectStatus Status { get; set; }
    public string OutputUrl { get; set; } = string.Empty;
    public string Configuration { get; set; } = "{}";  // JSON configuration
    public int GenerationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Navigation
    public List<Generation> Generations { get; set; } = [];

    private Project() { }

    /// <summary>
    /// Factory method to create a new project
    /// </summary>
    public static Project Create(Guid userId, string name, string description, Guid templateId, string? configuration = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required", nameof(userId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required", nameof(name));
        if (templateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required", nameof(templateId));

        return new Project
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = description,
            TemplateId = templateId,
            Status = ProjectStatus.Draft,
            GenerationCount = 0,
            Configuration = configuration ?? "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generate the project (create output)
    /// </summary>
    public void Generate(string outputUrl)
    {
        if (string.IsNullOrWhiteSpace(outputUrl))
            throw new ArgumentException("Output URL is required", nameof(outputUrl));

        Status = ProjectStatus.Generated;
        OutputUrl = outputUrl;
        GenerationCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Publish the project
    /// </summary>
    public void Publish()
    {
        if (Status != ProjectStatus.Generated)
            throw new InvalidOperationException("Project must be generated before publishing");

        Status = ProjectStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Archive the project
    /// </summary>
    public void Archive()
    {
        Status = ProjectStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Generation aggregate - represents a single generation/version of a project
/// </summary>
public sealed class Generation
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid TemplateId { get; set; }
    public GenerationStatus Status { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string Configuration { get; set; } = "{}";  // JSON configuration used for generation
    public decimal EstimatedCreditsUsed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    private Generation() { }

    /// <summary>
    /// Factory method to create a new generation
    /// </summary>
    public static Generation Create(Guid projectId, Guid templateId, string configuration, decimal estimatedCredits)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("ProjectId is required", nameof(projectId));
        if (templateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required", nameof(templateId));
        if (estimatedCredits < 0)
            throw new ArgumentException("Credits cannot be negative", nameof(estimatedCredits));

        return new Generation
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            TemplateId = templateId,
            Status = GenerationStatus.Pending,
            Configuration = configuration,
            EstimatedCreditsUsed = estimatedCredits,
            StartedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Mark generation as in progress
    /// </summary>
    public void MarkAsInProgress()
    {
        Status = GenerationStatus.InProgress;
    }

    /// <summary>
    /// Complete generation successfully
    /// </summary>
    public void CompleteSuccessfully(string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path is required", nameof(outputPath));

        Status = GenerationStatus.Completed;
        OutputPath = outputPath;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Mark generation as failed
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message is required", nameof(errorMessage));

        Status = GenerationStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Template types supported
/// </summary>
public enum TemplateType
{
    Blog = 0,
    Portfolio = 1,
    ECommerce = 2,
    LandingPage = 3,
    Documentation = 4,
    Corporate = 5,
    SaaS = 6,
    Custom = 7
}

/// <summary>
/// Project lifecycle statuses
/// </summary>
public enum ProjectStatus
{
    Draft = 0,
    Generating = 1,
    Generated = 2,
    Published = 3,
    Archived = 4,
    Failed = 5
}

/// <summary>
/// Generation execution statuses
/// </summary>
public enum GenerationStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}
