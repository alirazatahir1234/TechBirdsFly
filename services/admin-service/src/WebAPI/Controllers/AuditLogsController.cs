using Microsoft.AspNetCore.Mvc;
using TechBirdsFly.AdminService.Application.DTOs;
using TechBirdsFly.AdminService.Application.Interfaces;

namespace TechBirdsFly.AdminService.WebAPI.Controllers;

/// <summary>
/// Audit Logs Controller - Handles audit log querying and retrieval.
/// Provides access to audit trail with advanced filtering and pagination.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogApplicationService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogApplicationService auditLogService,
        ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get audit logs with optional filtering and pagination.
    /// </summary>
    /// <param name="adminUserId">Optional: Filter by admin user ID</param>
    /// <param name="action">Optional: Filter by action (e.g., "UserCreated")</param>
    /// <param name="resourceType">Optional: Filter by resource type (e.g., "User")</param>
    /// <param name="fromDate">Optional: Filter from date (inclusive)</param>
    /// <param name="toDate">Optional: Filter to date (inclusive)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paginated list of audit logs</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<AuditLogDto>>>> GetAuditLogs(
        [FromQuery] Guid? adminUserId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? resourceType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (pageNumber < 1)
            return BadRequest(ApiResponse<PaginatedResult<AuditLogDto>>.ErrorResponse(
                "Invalid request", new[] { "Page number must be at least 1" }));

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(ApiResponse<PaginatedResult<AuditLogDto>>.ErrorResponse(
                "Invalid request", new[] { "Page size must be between 1 and 100" }));

        // Validate date range
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            return BadRequest(ApiResponse<PaginatedResult<AuditLogDto>>.ErrorResponse(
                "Invalid request", new[] { "From date cannot be after to date" }));

        try
        {
            _logger.LogInformation(
                "Fetching audit logs with filters - UserId: {UserId}, Action: {Action}, ResourceType: {ResourceType}, DateRange: {FromDate} to {ToDate}, Page: {PageNumber}/{PageSize}",
                adminUserId, action, resourceType, fromDate, toDate, pageNumber, pageSize);

            var filter = new AuditLogFilterRequest
            {
                AdminUserId = adminUserId,
                Action = action,
                ResourceType = resourceType,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (items, totalCount) = await _auditLogService.GetAuditLogsAsync(filter, cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var result = new PaginatedResult<AuditLogDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };

            _logger.LogInformation("Audit logs retrieved successfully. Count: {Count}, TotalPages: {TotalPages}", items.Count(), totalPages);
            return Ok(ApiResponse<PaginatedResult<AuditLogDto>>.Success(result, "Audit logs retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<PaginatedResult<AuditLogDto>>.ErrorResponse("Failed to retrieve audit logs", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Get a specific audit log by ID.
    /// </summary>
    /// <param name="id">Audit log ID</param>
    /// <returns>Audit log details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AuditLogDto>>> GetAuditLogById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AuditLogDto>.ErrorResponse("Invalid audit log ID", new[] { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Fetching audit log with ID: {AuditLogId}", id);
            var auditLog = await _auditLogService.GetAuditLogAsync(id, cancellationToken);

            if (auditLog == null)
            {
                _logger.LogWarning("Audit log not found: {AuditLogId}", id);
                return NotFound(ApiResponse<AuditLogDto>.ErrorResponse("Audit log not found", new[] { $"No audit log with ID {id}" }));
            }

            return Ok(ApiResponse<AuditLogDto>.Success(auditLog, "Audit log retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit log {AuditLogId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AuditLogDto>.ErrorResponse("Failed to retrieve audit log", new[] { ex.Message }));
        }
    }
}

/// <summary>
/// Paginated result DTO for audit log queries.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PaginatedResult<T> where T : class
{
    /// <summary>
    /// Items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Current page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; set; }
}
