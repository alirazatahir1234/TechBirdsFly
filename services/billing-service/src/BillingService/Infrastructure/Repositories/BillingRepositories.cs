namespace BillingService.Infrastructure.Repositories;

using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Invoice repository implementation
/// </summary>
public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly BillingDbContext _context;
    private readonly ILogger<InvoiceRepository> _logger;

    public InvoiceRepository(BillingDbContext context, ILogger<InvoiceRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.UserId == userId)
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<InvoiceStatus>(status, true, out var invoiceStatus))
            throw new ArgumentException($"Invalid invoice status: {status}");

        return await _context.Invoices
            .Where(i => i.Status == invoiceStatus)
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(id, cancellationToken);
        if (invoice is not null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// Payment repository implementation
/// </summary>
public sealed class PaymentRepository : IPaymentRepository
{
    private readonly BillingDbContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(BillingDbContext context, ILogger<PaymentRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            throw new ArgumentException($"Invalid payment status: {status}");

        return await _context.Payments
            .Where(p => p.Status == paymentStatus)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(id, cancellationToken);
        if (payment is not null)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// Plan repository implementation
/// </summary>
public sealed class PlanRepository : IPlanRepository
{
    private readonly BillingDbContext _context;
    private readonly ILogger<PlanRepository> _logger;

    public PlanRepository(BillingDbContext context, ILogger<PlanRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Plan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Plan> CreateAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        _context.Plans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public async Task<Plan> UpdateAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        _context.Plans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await GetByIdAsync(id, cancellationToken);
        if (plan is not null)
        {
            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// Subscription repository implementation
/// </summary>
public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly BillingDbContext _context;
    private readonly ILogger<SubscriptionRepository> _logger;

    public SubscriptionRepository(BillingDbContext context, ILogger<SubscriptionRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Invoices)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Invoices)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Invoices)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status != SubscriptionStatus.Cancelled, cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SubscriptionStatus>(status, true, out var subscriptionStatus))
            throw new ArgumentException($"Invalid subscription status: {status}");

        return await _context.Subscriptions
            .Where(s => s.Status == subscriptionStatus)
            .Include(s => s.Plan)
            .Include(s => s.Invoices)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subscription>> GetTrialEndingAsync(int daysThreshold, CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
        return await _context.Subscriptions
            .Where(s => s.IsOnTrial && s.TrialEndDate <= thresholdDate && s.Status != SubscriptionStatus.Cancelled)
            .Include(s => s.Plan)
            .Include(s => s.Invoices)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription> CreateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await GetByIdAsync(id, cancellationToken);
        if (subscription is not null)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
