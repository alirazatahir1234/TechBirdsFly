namespace BillingService.Application.Services;

using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using BillingService.Domain.Events;
using Microsoft.Extensions.Logging;

/// <summary>
/// Payment application service
/// </summary>
public sealed class PaymentApplicationService : IPaymentApplicationService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PaymentApplicationService> _logger;

    public PaymentApplicationService(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        IEventPublisher eventPublisher,
        ILogger<PaymentApplicationService> logger)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating payment for invoice: {InvoiceId}, Amount: {Amount}", request.InvoiceId, request.Amount);

        try
        {
            var payment = Payment.Create(request.UserId, request.InvoiceId, request.Amount, request.PaymentMethod);
            var created = await _paymentRepository.CreateAsync(payment, cancellationToken);

            _logger.LogInformation("Payment created: {PaymentId}", created.Id);
            return MapToDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for invoice: {InvoiceId}", request.InvoiceId);
            throw;
        }
    }

    public async Task<PaymentDto> ProcessPaymentAsync(Guid paymentId, ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing payment: {PaymentId}", paymentId);

        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment is null)
                throw new KeyNotFoundException($"Payment {paymentId} not found");

            // Simulate payment processing
            payment.MarkAsCompleted(request.ExternalTransactionId);
            var updated = await _paymentRepository.UpdateAsync(payment, cancellationToken);

            // Update invoice
            var invoice = await _invoiceRepository.GetByIdAsync(payment.InvoiceId, cancellationToken);
            if (invoice is not null)
            {
                invoice.RecordPayment(payment);
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            }

            var @event = new PaymentProcessedEvent(paymentId, payment.UserId, payment.InvoiceId, payment.Amount, payment.PaymentMethod, true);
            await _eventPublisher.PublishEventAsync(@event, cancellationToken);

            _logger.LogInformation("Payment processed successfully: {PaymentId}", paymentId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<PaymentDto?> GetPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching payment: {PaymentId}", paymentId);
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        return payment is not null ? MapToDto(payment) : null;
    }

    public async Task<IEnumerable<PaymentDto>> GetInvoicePaymentsAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching payments for invoice: {InvoiceId}", invoiceId);
        var payments = await _paymentRepository.GetByInvoiceIdAsync(invoiceId, cancellationToken);
        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching payments for user: {UserId}", userId);
        var payments = await _paymentRepository.GetByUserIdAsync(userId, cancellationToken);
        return payments.Select(MapToDto);
    }

    public async Task<PaymentDto> RefundPaymentAsync(Guid paymentId, RefundPaymentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refunding payment: {PaymentId}", paymentId);

        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment is null)
                throw new KeyNotFoundException($"Payment {paymentId} not found");

            payment.Refund(request.Reason);
            var updated = await _paymentRepository.UpdateAsync(payment, cancellationToken);

            _logger.LogInformation("Payment refunded: {PaymentId}", paymentId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<PaymentDto> RetryPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrying payment: {PaymentId}", paymentId);

        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment is null)
                throw new KeyNotFoundException($"Payment {paymentId} not found");

            if (!payment.CanRetry())
                throw new InvalidOperationException("Payment cannot be retried");

            // Retry logic would go here
            var updated = await _paymentRepository.UpdateAsync(payment, cancellationToken);

            _logger.LogInformation("Payment retry initiated: {PaymentId}", paymentId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment: {PaymentId}", paymentId);
            throw;
        }
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.UserId,
            payment.InvoiceId,
            payment.Amount,
            payment.Currency,
            payment.Status.ToString(),
            payment.PaymentMethod,
            payment.ExternalTransactionId,
            payment.ExternalPaymentGateway,
            payment.ProcessedAt,
            payment.CompletedAt,
            payment.FailureReason,
            payment.RetryCount);
    }
}

/// <summary>
/// Subscription application service
/// </summary>
public sealed class SubscriptionApplicationService : ISubscriptionApplicationService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SubscriptionApplicationService> _logger;

    public SubscriptionApplicationService(
        ISubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository,
        IEventPublisher eventPublisher,
        ILogger<SubscriptionApplicationService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating subscription for user: {UserId}, Plan: {PlanId}", request.UserId, request.PlanId);

        try
        {
            var plan = await _planRepository.GetByIdAsync(request.PlanId, cancellationToken);
            if (plan is null)
                throw new KeyNotFoundException($"Plan {request.PlanId} not found");

            var subscription = Subscription.Create(request.UserId, request.PlanId, request.TrialDays ?? plan.TrialDays);
            var created = await _subscriptionRepository.CreateAsync(subscription, cancellationToken);

            var @event = new SubscriptionCreatedEvent(created.Id, created.UserId, created.PlanId, created.IsOnTrial, created.TrialEndDate);
            await _eventPublisher.PublishEventAsync(@event, cancellationToken);

            _logger.LogInformation("Subscription created: {SubscriptionId}", created.Id);
            return MapToDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user: {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<SubscriptionDto?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching subscription: {SubscriptionId}", subscriptionId);
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
        return subscription is not null ? MapToDto(subscription) : null;
    }

    public async Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching subscription for user: {UserId}", userId);
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        return subscription is not null ? MapToDto(subscription) : null;
    }

    public async Task<SubscriptionDto> CancelSubscriptionAsync(Guid subscriptionId, CancelSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling subscription: {SubscriptionId}", subscriptionId);

        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription is null)
                throw new KeyNotFoundException($"Subscription {subscriptionId} not found");

            subscription.Cancel(request.Reason);
            var updated = await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            var @event = new SubscriptionCancelledEvent(subscriptionId, subscription.UserId, subscription.PlanId, request.Reason);
            await _eventPublisher.PublishEventAsync(@event, cancellationToken);

            _logger.LogInformation("Subscription cancelled: {SubscriptionId}", subscriptionId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription: {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task<SubscriptionDto> RenewSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Renewing subscription: {SubscriptionId}", subscriptionId);

        try
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription is null)
                throw new KeyNotFoundException($"Subscription {subscriptionId} not found");

            subscription.Renew();
            var updated = await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);

            var @event = new SubscriptionRenewedEvent(subscriptionId, subscription.UserId, subscription.PlanId, updated.NextBillingDate.Value);
            await _eventPublisher.PublishEventAsync(@event, cancellationToken);

            _logger.LogInformation("Subscription renewed: {SubscriptionId}", subscriptionId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription: {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    private static SubscriptionDto MapToDto(Subscription subscription)
    {
        return new SubscriptionDto(
            subscription.Id,
            subscription.UserId,
            subscription.PlanId,
            subscription.Status.ToString(),
            subscription.StartDate,
            subscription.EndDate,
            subscription.NextBillingDate,
            subscription.IsOnTrial,
            subscription.TrialEndDate,
            subscription.CancellationReason,
            subscription.IsTrialExpired,
            subscription.NeedsRenewal);
    }
}
