namespace ExportService.Domain.Entities;

/// <summary>
/// Represents an exported website file in the system
/// </summary>
public class ExportFile
{
    /// <summary>
    /// Unique identifier for the export
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the project being exported
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Target framework for code generation (html, react, nextjs)
    /// </summary>
    public string Framework { get; set; } = string.Empty;

    /// <summary>
    /// Physical file path where the zip is stored
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// URL for downloading the exported file
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Size of the exported zip file in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Timestamp when export was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who requested the export
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Current status of the export (pending, completed, failed)
    /// </summary>
    public ExportStatus Status { get; set; } = ExportStatus.Pending;

    /// <summary>
    /// Error message if export failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Status enumeration for export operations
/// </summary>
public enum ExportStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}
