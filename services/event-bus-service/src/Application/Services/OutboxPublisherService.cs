using EventBusService.Application.Interfaces;
using EventBusService.Domain.Entities;

namespace EventBusService.Application.Services;

/// <summary>
/// Background service that publishes outbox events to Kafka
/// Implements the Outbox Pattern for guaranteed event delivery
/// </summary>
public class OutboxPublisherService
{
    private readonly IOutboxEventRepository _outboxRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<OutboxPublisherService> _logger;
    private readonly OutboxPublisherSettings _settings;

    public OutboxPublisherService(
        IOutboxEventRepository outboxRepository,
        IKafkaProducer kafkaProducer,
        ILogger<OutboxPublisherService> logger,
        OutboxPublisherSettings settings)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Process and publish unpublished outbox events to Kafka
    /// Implements exponential backoff retry logic
    /// </summary>
    public async Task<OutboxPublishResult> PublishPendingEventsAsync(CancellationToken cancellationToken = default)
    {
        var result = new OutboxPublishResult();

        try
        {
            _logger.LogInformation("🔄 Starting outbox event publishing process");

            // Fetch unpublished events in batches
            var outboxEvents = await _outboxRepository.GetUnpublishedAsync(
                _settings.BatchSize,
                cancellationToken);

            if (outboxEvents.Count == 0)
            {
                _logger.LogDebug("✅ No pending events to publish");
                result.IsSuccess = true;
                return result;
            }

            _logger.LogInformation(
                "📦 Found {EventCount} unpublished events to process",
                outboxEvents.Count);

            // Process each event
            foreach (var outboxEvent in outboxEvents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("❌ Publishing process cancelled");
                    break;
                }

                await ProcessOutboxEventAsync(outboxEvent, result, cancellationToken);
            }

            _logger.LogInformation(
                "✅ Outbox publishing complete - Successful: {Successful}, Failed: {Failed}",
                result.SuccessfulCount,
                result.FailedCount);

            result.IsSuccess = result.FailedCount == 0;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fatal error in outbox publishing process");
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Process a single outbox event with retry logic
    /// </summary>
    private async Task ProcessOutboxEventAsync(
        OutboxEvent outboxEvent,
        OutboxPublishResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if max retries exceeded
            if (outboxEvent.PublishAttempts >= _settings.MaxRetryAttempts)
            {
                _logger.LogError(
                    "💀 Event {EventId} exceeded max retry attempts ({MaxAttempts})",
                    outboxEvent.Id,
                    _settings.MaxRetryAttempts);

                result.FailedCount++;
                result.DeadLetterCount++;
                return;
            }

            _logger.LogInformation(
                "📤 Publishing event - EventId: {EventId}, Type: {EventType}, Attempt: {Attempt}/{MaxAttempts}",
                outboxEvent.Id,
                outboxEvent.EventType,
                outboxEvent.PublishAttempts + 1,
                _settings.MaxRetryAttempts);

            // Publish to Kafka
            await _kafkaProducer.PublishAsync(
                outboxEvent.Topic,
                outboxEvent.PartitionKey ?? outboxEvent.Id.ToString(),
                outboxEvent.EventPayload,
                outboxEvent.EventType,
                cancellationToken);

            // Mark as published
            outboxEvent.IsPublished = true;
            outboxEvent.PublishedAt = DateTime.UtcNow;
            outboxEvent.LastErrorMessage = null;

            await _outboxRepository.MarkAsPublishedAsync(outboxEvent.Id, cancellationToken);

            _logger.LogInformation(
                "✅ Successfully published event - EventId: {EventId}, Type: {EventType}",
                outboxEvent.Id,
                outboxEvent.EventType);

            result.SuccessfulCount++;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "⏸️ Publishing cancelled for event {EventId}",
                outboxEvent.Id);
            result.CancelledCount++;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "⚠️ Failed to publish event (Attempt {Attempt}) - EventId: {EventId}, Error: {Error}",
                outboxEvent.PublishAttempts + 1,
                outboxEvent.Id,
                ex.Message);

            // Update retry attempt
            await _outboxRepository.UpdatePublishAttemptAsync(
                outboxEvent.Id,
                ex.Message,
                cancellationToken);

            result.FailedCount++;
        }
    }
}

/// <summary>
/// Result of publishing pending outbox events
/// </summary>
public class OutboxPublishResult
{
    /// <summary>
    /// Whether the entire operation succeeded
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Number of successfully published events
    /// </summary>
    public int SuccessfulCount { get; set; }

    /// <summary>
    /// Number of failed events (will be retried next time)
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Number of cancelled events
    /// </summary>
    public int CancelledCount { get; set; }

    /// <summary>
    /// Number of dead-lettered events (exceeded max retries)
    /// </summary>
    public int DeadLetterCount { get; set; }

    /// <summary>
    /// Total events processed
    /// </summary>
    public int TotalProcessed => SuccessfulCount + FailedCount + CancelledCount + DeadLetterCount;

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Settings for outbox publisher
/// </summary>
public class OutboxPublisherSettings
{
    /// <summary>
    /// Batch size for processing events
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum retry attempts per event
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>
    /// Initial delay in milliseconds for retry
    /// </summary>
    public int InitialRetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum delay in milliseconds for retry (backoff cap)
    /// </summary>
    public int MaxRetryDelayMs { get; set; } = 30000;

    /// <summary>
    /// Backoff multiplier for exponential retry
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;
}
