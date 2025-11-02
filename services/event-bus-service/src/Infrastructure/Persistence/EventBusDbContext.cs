using Microsoft.EntityFrameworkCore;
using EventBusService.Domain.Entities;

namespace EventBusService.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Event Bus Service
/// </summary>
public class EventBusDbContext : DbContext
{
    public EventBusDbContext(DbContextOptions<EventBusDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    public DbSet<EventSubscription> EventSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure OutboxEvent
        modelBuilder.Entity<OutboxEvent>(builder =>
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.EventType)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(o => o.EventPayload)
                .IsRequired();

            builder.Property(o => o.Topic)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(o => o.PartitionKey)
                .HasMaxLength(256);

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.Property(o => o.OccurredAt)
                .IsRequired();

            // Indexes for efficient querying
            builder.HasIndex(o => o.IsPublished)
                .HasDatabaseName("IX_OutboxEvent_IsPublished");

            builder.HasIndex(o => o.EventType)
                .HasDatabaseName("IX_OutboxEvent_EventType");

            builder.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IX_OutboxEvent_CreatedAt");

            builder.Ignore(o => o.DomainEvents);
        });

        // Configure EventSubscription
        modelBuilder.Entity<EventSubscription>(builder =>
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ServiceName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.WebhookUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(e => e.IsActive)
                .IsRequired();

            builder.Property(e => e.RetryCount)
                .HasDefaultValue(3);

            builder.Property(e => e.TimeoutSeconds)
                .HasDefaultValue(30);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(e => e.EventType)
                .HasDatabaseName("IX_EventSubscription_EventType");

            builder.HasIndex(e => e.ServiceName)
                .HasDatabaseName("IX_EventSubscription_ServiceName");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_EventSubscription_IsActive");

            builder.Ignore(e => e.DomainEvents);
        });
    }
}
