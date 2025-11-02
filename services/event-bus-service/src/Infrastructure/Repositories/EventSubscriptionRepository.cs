using Microsoft.EntityFrameworkCore;
using EventBusService.Domain.Entities;
using EventBusService.Application.Interfaces;
using EventBusService.Infrastructure.Persistence;

namespace EventBusService.Infrastructure.Repositories;

/// <summary>
/// Repository for event subscriptions
/// </summary>
public class EventSubscriptionRepository : IEventSubscriptionRepository
{
    private readonly EventBusDbContext _context;

    public EventSubscriptionRepository(EventBusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EventSubscription subscription, CancellationToken cancellationToken = default)
    {
        _context.EventSubscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<EventSubscription>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await _context.EventSubscriptions
            .Where(s => s.EventType == eventType && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EventSubscription>> GetByServiceAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return await _context.EventSubscriptions
            .Where(s => s.ServiceName == serviceName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EventSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EventSubscriptions
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<EventSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EventSubscriptions.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(EventSubscription subscription, CancellationToken cancellationToken = default)
    {
        _context.EventSubscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.EventSubscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken: cancellationToken);
        if (subscription != null)
        {
            _context.EventSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateDeliveryStatusAsync(Guid subscriptionId, bool success, string? failureReason = null, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.EventSubscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken: cancellationToken);
        if (subscription != null)
        {
            if (success)
            {
                subscription.LastDeliveredAt = DateTime.UtcNow;
                subscription.FailureReason = null;
            }
            else
            {
                subscription.LastFailedAt = DateTime.UtcNow;
                subscription.FailureReason = failureReason;
            }

            _context.EventSubscriptions.Update(subscription);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
