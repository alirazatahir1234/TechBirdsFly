namespace BillingService.Application.Services;

using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using BillingService.Domain.Events;
using Microsoft.Extensions.Logging;

/// <summary>
/// Invoice application service
/// </summary>
public sealed class InvoiceApplicationService : IInvoiceApplicationService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<InvoiceApplicationService> _logger;

    public InvoiceApplicationService(
        IInvoiceRepository invoiceRepository,
        IEventPublisher eventPublisher,
        ILogger<InvoiceApplicationService> logger)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating invoice for user: {UserId}, Amount: {Amount}", request.UserId, request.Amount);

        try
        {
            var invoice = Invoice.Create(request.UserId, request.Amount, request.TaxAmount, request.Description, request.SubscriptionId);
            var created = await _invoiceRepository.CreateAsync(invoice, cancellationToken);

            var @event = new InvoiceCreatedEvent(created.Id, created.UserId, created.Amount, created.TaxAmount, created.Description);
            await _eventPublisher.PublishEventAsync(@event, cancellationToken);

            _logger.LogInformation("Invoice created successfully: {InvoiceId}", created.Id);
            return MapToDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for user: {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching invoice: {InvoiceId}", invoiceId);
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        return invoice is not null ? MapToDto(invoice) : null;
    }

    public async Task<IEnumerable<InvoiceDto>> GetUserInvoicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching invoices for user: {UserId}", userId);
        var invoices = await _invoiceRepository.GetByUserIdAsync(userId, cancellationToken);
        return invoices.Select(MapToDto);
    }

    public async Task<InvoiceDto> IssueInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Issuing invoice: {InvoiceId}", invoiceId);

        try
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
            if (invoice is null)
                throw new KeyNotFoundException($"Invoice {invoiceId} not found");

            invoice.Issue();
            var updated = await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

            var @event = new InvoiceIssuedEvent(updated.Id, updated.UserId, updated.TotalAmount, updated.DueDate.Value);
            await _eventPublisher.PublishEventAsync(@event, cancellationToken);

            _logger.LogInformation("Invoice issued successfully: {InvoiceId}", invoiceId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing invoice: {InvoiceId}", invoiceId);
            throw;
        }
    }

    public async Task<InvoiceDto> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating invoice: {InvoiceId}", invoiceId);

        try
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
            if (invoice is null)
                throw new KeyNotFoundException($"Invoice {invoiceId} not found");

            if (invoice.Status != InvoiceStatus.Draft)
                throw new InvalidOperationException("Only draft invoices can be updated");

            // Update properties (simplified - in real implementation, would update individual fields)
            var updated = await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

            _logger.LogInformation("Invoice updated successfully: {InvoiceId}", invoiceId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice: {InvoiceId}", invoiceId);
            throw;
        }
    }

    public async Task DeleteInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting invoice: {InvoiceId}", invoiceId);

        try
        {
            await _invoiceRepository.DeleteAsync(invoiceId, cancellationToken);
            _logger.LogInformation("Invoice deleted successfully: {InvoiceId}", invoiceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice: {InvoiceId}", invoiceId);
            throw;
        }
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.UserId,
            invoice.SubscriptionId,
            invoice.Amount,
            invoice.TaxAmount,
            invoice.TotalAmount,
            invoice.Currency,
            invoice.Description,
            invoice.Status.ToString(),
            invoice.IssuedDate,
            invoice.DueDate,
            invoice.PaidDate,
            invoice.GetRemainingBalance(),
            invoice.IsOverdue,
            invoice.LineItems.Select(li => new InvoiceLineItemDto(li.Id, li.Description, li.Quantity, li.UnitPrice, li.Amount)).ToList());
    }
}
