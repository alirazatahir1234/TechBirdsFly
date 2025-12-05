namespace PublishService.Domain.Entities;

/// <summary>
/// Represents a published website record
/// </summary>
public class PublishRecord
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = "";
    public string? Url { get; private set; }
    public string Status { get; private set; } = "PENDING";
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }

    public PublishRecord(Guid projectId, Guid userId, string provider)
    {
        ProjectId = projectId;
        UserId = userId;
        Provider = provider;
    }

    public void MarkSuccess(string url)
    {
        Status = "SUCCESS";
        Url = url;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string? errorMessage = null)
    {
        Status = "FAILED";
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkInProgress()
    {
        Status = "IN_PROGRESS";
    }

    public bool IsCompleted => Status == "SUCCESS" || Status == "FAILED";
}
