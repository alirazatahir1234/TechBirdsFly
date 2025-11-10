namespace GeneratorService.Application.DTOs;

// ============================================================================
// TEMPLATE DTOs
// ============================================================================

public record CreateTemplateRequest(
    string Name,
    string Description,
    string Type,
    string Category,
    string Content,
    string? ThumbnailUrl = null);

public record UpdateTemplateRequest(
    string? Name = null,
    string? Description = null,
    string? Content = null,
    string? ThumbnailUrl = null);

public record TemplateDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    string Category,
    string ThumbnailUrl,
    bool IsActive,
    int UseCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ============================================================================
// PROJECT DTOs
// ============================================================================

public record CreateProjectRequest(
    Guid UserId,
    string Name,
    string Description,
    Guid TemplateId,
    string? Configuration = null);

public record UpdateProjectRequest(
    string? Name = null,
    string? Description = null,
    string? Configuration = null);

public record ProjectDto(
    Guid Id,
    Guid UserId,
    string Name,
    string Description,
    Guid TemplateId,
    string Status,
    string OutputUrl,
    int GenerationCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt = null);

// ============================================================================
// GENERATION DTOs
// ============================================================================

public record GenerationDto(
    Guid Id,
    Guid ProjectId,
    Guid TemplateId,
    string Status,
    string OutputPath,
    string ErrorMessage,
    decimal EstimatedCreditsUsed,
    DateTime StartedAt,
    DateTime? CompletedAt = null);

public record GenerateProjectRequest(
    Guid ProjectId,
    string AiPrompt,
    string? CustomConfiguration = null);

public record PublishProjectRequest(
    Guid ProjectId);

public record ArchiveProjectRequest(
    Guid ProjectId);

// ============================================================================
// UTILITY DTOs
// ============================================================================

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message,
    List<string>? Errors)
{
    public static ApiResponse<T> SuccessResponse(T data, string message = "Success") =>
        new(true, data, message, null);

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null) =>
        new(false, default, message, errors ?? new List<string>());
}

public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public static PaginatedResult<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount) =>
        new(
            items,
            pageNumber,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
}
