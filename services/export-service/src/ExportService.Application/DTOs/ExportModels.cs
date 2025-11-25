namespace ExportService.Application.Models;

/// <summary>
/// Data transfer object for project information from GeneratorService
/// </summary>
public class ProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
    public string Css { get; set; } = string.Empty;
    public string Json { get; set; } = string.Empty;
    public Dictionary<string, object> Components { get; set; } = new();
}

/// <summary>
/// Data transfer object for export result
/// </summary>
public class ExportResult
{
    public Guid ExportId { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for export request
/// </summary>
public class ExportRequestDto
{
    public string ProjectId { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
}
