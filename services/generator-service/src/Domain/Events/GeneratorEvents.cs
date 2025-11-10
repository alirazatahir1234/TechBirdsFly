namespace GeneratorService.Domain.Events;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent
{
    public Guid AggregateId { get; protected set; }
    public Guid CorrelationId { get; protected set; }
    public DateTime OccurredAt { get; protected set; }
    public string EventType { get; protected set; } = string.Empty;
}

/// <summary>
/// Event: Template created
/// </summary>
public class TemplateCreatedEvent : DomainEvent
{
    public string TemplateName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public TemplateCreatedEvent(Guid templateId, string templateName, string category, string type)
    {
        AggregateId = templateId;
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = nameof(TemplateCreatedEvent);
        TemplateName = templateName;
        Category = category;
        Type = type;
    }
}

/// <summary>
/// Event: Project created
/// </summary>
public class ProjectCreatedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }

    public ProjectCreatedEvent(Guid projectId, Guid userId, string projectName, Guid templateId)
    {
        AggregateId = projectId;
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = nameof(ProjectCreatedEvent);
        UserId = userId;
        ProjectName = projectName;
        TemplateId = templateId;
    }
}

/// <summary>
/// Event: Generation started
/// </summary>
public class GenerationStartedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public Guid TemplateId { get; set; }
    public decimal CreditsUsed { get; set; }

    public GenerationStartedEvent(Guid generationId, Guid projectId, Guid templateId, decimal creditsUsed)
    {
        AggregateId = generationId;
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = nameof(GenerationStartedEvent);
        ProjectId = projectId;
        TemplateId = templateId;
        CreditsUsed = creditsUsed;
    }
}

/// <summary>
/// Event: Generation completed
/// </summary>
public class GenerationCompletedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public decimal ActualCreditsUsed { get; set; }

    public GenerationCompletedEvent(Guid generationId, Guid projectId, string outputPath, decimal actualCreditsUsed)
    {
        AggregateId = generationId;
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = nameof(GenerationCompletedEvent);
        ProjectId = projectId;
        OutputPath = outputPath;
        ActualCreditsUsed = actualCreditsUsed;
    }
}

/// <summary>
/// Event: Generation failed
/// </summary>
public class GenerationFailedEvent : DomainEvent
{
    public Guid ProjectId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public GenerationFailedEvent(Guid generationId, Guid projectId, string errorMessage)
    {
        AggregateId = generationId;
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = nameof(GenerationFailedEvent);
        ProjectId = projectId;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Event: Project published
/// </summary>
public class ProjectPublishedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string OutputUrl { get; set; } = string.Empty;

    public ProjectPublishedEvent(Guid projectId, Guid userId, string projectName, string outputUrl)
    {
        AggregateId = projectId;
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        EventType = nameof(ProjectPublishedEvent);
        UserId = userId;
        ProjectName = projectName;
        OutputUrl = outputUrl;
    }
}
