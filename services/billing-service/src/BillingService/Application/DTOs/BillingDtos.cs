namespace BillingService.Application.DTOs;

// ============================================================================
// INVOICE DTOs
// ============================================================================

public record CreateInvoiceRequest(
    Guid UserId,
    decimal Amount,
    decimal TaxAmount,
    string Description,
    Guid? SubscriptionId = null);

public record UpdateInvoiceRequest(
    decimal Amount,
    decimal TaxAmount,
    string Description);

public record InvoiceDto(
    Guid Id,
    Guid UserId,
    Guid? SubscriptionId,
    decimal Amount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    string Description,
    string Status,
    DateTime IssuedDate,
    DateTime? DueDate,
    DateTime? PaidDate,
    decimal RemainingBalance,
    bool IsOverdue,
    List<InvoiceLineItemDto> LineItems);

public record InvoiceLineItemDto(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount);

// ============================================================================
// PAYMENT DTOs
// ============================================================================

public record CreatePaymentRequest(
    Guid UserId,
    Guid InvoiceId,
    decimal Amount,
    string PaymentMethod,
    string? ExternalTransactionId = null);

public record ProcessPaymentRequest(
    decimal Amount,
    string PaymentMethod,
    string? ExternalGateway = "manual",
    string? ExternalTransactionId = null);

public record RefundPaymentRequest(
    string Reason);

public record PaymentDto(
    Guid Id,
    Guid UserId,
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    string Status,
    string PaymentMethod,
    string? ExternalTransactionId,
    string? ExternalPaymentGateway,
    DateTime ProcessedAt,
    DateTime? CompletedAt,
    string? FailureReason,
    int RetryCount);

// ============================================================================
// PLAN DTOs
// ============================================================================

public record CreatePlanRequest(
    string Name,
    string Description,
    string Type,
    decimal Price,
    string BillingCycle,
    int? TrialDays = null,
    Dictionary<string, int?>? Features = null);

public record UpdatePlanRequest(
    string Name,
    string Description,
    decimal Price,
    int? TrialDays = null,
    Dictionary<string, int?>? Features = null);

public record PlanDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    decimal Price,
    string Currency,
    string BillingCycle,
    int? TrialDays,
    bool IsActive,
    string FeaturesJson,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ============================================================================
// SUBSCRIPTION DTOs
// ============================================================================

public record CreateSubscriptionRequest(
    Guid UserId,
    Guid PlanId,
    int? TrialDays = null);

public record UpdateSubscriptionRequest(
    Guid PlanId);

public record CancelSubscriptionRequest(
    string? Reason = null);

public record SubscriptionDto(
    Guid Id,
    Guid UserId,
    Guid PlanId,
    string Status,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? NextBillingDate,
    bool IsOnTrial,
    DateTime? TrialEndDate,
    string? CancellationReason,
    bool IsTrialExpired,
    bool NeedsRenewal);

// ============================================================================
// GENERIC RESPONSE TYPES
// ============================================================================

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string Message,
    string[]? Errors = null)
{
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        => new(true, data, message);

    public static ApiResponse<T> ErrorResponse(string message, params string[] errors)
        => new(false, default, message, errors);
}

public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage)
{
    public static PaginatedResult<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var hasPreviousPage = pageNumber > 1;
        var hasNextPage = pageNumber < totalPages;

        return new(items, pageNumber, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage);
    }
}
