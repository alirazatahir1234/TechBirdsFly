namespace PublishService.Application.DTOs;

/// <summary>
/// Request DTO for deploying a website
/// </summary>
public class DeployRequestDto
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Html { get; set; } = "";
    public string Provider { get; set; } = ""; // vercel | netlify | techbirdsfly
    public string Token { get; set; } = ""; // provider token
}

/// <summary>
/// Response DTO for deployment
/// </summary>
public class DeployResponseDto
{
    public Guid PublishRecordId { get; set; }
    public string Url { get; set; } = "";
    public string Status { get; set; } = "";
}

/// <summary>
/// DTO for publish status
/// </summary>
public class PublishStatusDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Provider { get; set; } = "";
    public string Url { get; set; } = "";
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// DTO for publish history
/// </summary>
public class PublishHistoryDto
{
    public List<PublishStatusDto> Records { get; set; } = new();
    public int Total { get; set; }
}
