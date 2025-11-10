namespace BillingService.Application.Interfaces;

using BillingService.Domain.Entities;
using BillingService.Application.DTOs;

// ============================================================================
// REPOSITORY INTERFACES
// ============================================================================

/// <summary>
/// Invoice repository interface
/// </summary>
public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment repository interface
/// </summary>
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Plan repository interface
/// </summary>
public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Plan>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Plan> CreateAsync(Plan plan, CancellationToken cancellationToken = default);
    Task<Plan> UpdateAsync(Plan plan, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Subscription repository interface
/// </summary>
public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subscription>> GetTrialEndingAsync(int daysThreshold, CancellationToken cancellationToken = default);
    Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

// ============================================================================
// APPLICATION SERVICE INTERFACES
// ============================================================================

/// <summary>
/// Invoice application service interface
/// </summary>
public interface IInvoiceApplicationService
{
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvoiceDto>> GetUserInvoicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> IssueInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task DeleteInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment application service interface
/// </summary>
public interface IPaymentApplicationService
{
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto> ProcessPaymentAsync(Guid paymentId, ProcessPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto?> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentDto>> GetInvoicePaymentsAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentDto> RefundPaymentAsync(Guid paymentId, RefundPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto> RetryPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Plan application service interface
/// </summary>
public interface IPlanApplicationService
{
    Task<PlanDto> CreatePlanAsync(CreatePlanRequest request, CancellationToken cancellationToken = default);
    Task<PlanDto?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanDto>> GetAllPlansAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<PlanDto> UpdatePlanAsync(Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(Guid planId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Subscription application service interface
/// </summary>
public interface ISubscriptionApplicationService
{
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> CancelSubscriptionAsync(Guid subscriptionId, CancelSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> RenewSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}

// ============================================================================
// EXTERNAL SERVICE INTERFACES
// ============================================================================

/// <summary>
/// Payment gateway service for processing external payments
/// </summary>
public interface IPaymentGatewayService
{
    Task<PaymentResult> ProcessPaymentAsync(string token, decimal amount, string currency, string description, CancellationToken cancellationToken = default);
    Task<RefundResult> RefundPaymentAsync(string transactionId, decimal amount, string reason, CancellationToken cancellationToken = default);
}

public record PaymentResult(
    bool Success,
    string? TransactionId,
    string? ErrorMessage);

public record RefundResult(
    bool Success,
    string? RefundId,
    string? ErrorMessage);

/// <summary>
/// Event publisher service for publishing domain events
/// </summary>
public interface IEventPublisher
{
    Task PublishEventAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
