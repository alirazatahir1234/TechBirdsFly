using Microsoft.EntityFrameworkCore;
using EventBusService.Domain.Entities;
using EventBusService.Application.Interfaces;
using EventBusService.Infrastructure.Persistence;

namespace EventBusService.Infrastructure.Repositories;

/// <summary>
/// Repository for outbox events
/// </summary>
public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly EventBusDbContext _context;

    public OutboxEventRepository(EventBusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {
        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<OutboxEvent>> GetUnpublishedAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents
            .Where(o => !o.IsPublished)
            .OrderBy(o => o.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] { eventId }, cancellationToken: cancellationToken);
        if (outboxEvent != null)
        {
            outboxEvent.IsPublished = true;
            outboxEvent.PublishedAt = DateTime.UtcNow;
            _context.OutboxEvents.Update(outboxEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdatePublishAttemptAsync(Guid eventId, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var outboxEvent = await _context.OutboxEvents.FindAsync(new object[] { eventId }, cancellationToken: cancellationToken);
        if (outboxEvent != null)
        {
            outboxEvent.PublishAttempts++;
            outboxEvent.LastErrorMessage = errorMessage;
            _context.OutboxEvents.Update(outboxEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<OutboxEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxEvents.FindAsync(new object[] { eventId }, cancellationToken: cancellationToken);
    }

    public async Task<List<OutboxEvent>> GetByEventTypeAsync(string eventType, int maxAge = 7, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-maxAge);
        return await _context.OutboxEvents
            .Where(o => o.EventType == eventType && o.CreatedAt >= cutoffDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
