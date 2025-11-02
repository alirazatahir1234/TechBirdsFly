using TechBirdsFly.Shared.Events.Contracts;
using TechBirdsFly.Shared.Events.Serialization;
using EventBusService.Application.DTOs;
using EventBusService.Application.Interfaces;
using EventBusService.Domain.Entities;
using System.Text.Json;

namespace EventBusService.Application.Services;

/// <summary>
/// Service for publishing events to the outbox
/// Implements the outbox pattern for guaranteed delivery
/// </summary>
public class PublishEventService
{
    private readonly IOutboxEventRepository _outboxRepository;
    private readonly ILogger<PublishEventService> _logger;

    public PublishEventService(
        IOutboxEventRepository outboxRepository,
        ILogger<PublishEventService> logger)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publish an event to the outbox
    /// </summary>
    /// <param name="request">Event publishing request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Publishing result with event ID</returns>
    public async Task<PublishEventResponse> PublishEventAsync(
        PublishEventRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate request
            if (request == null)
            {
                _logger.LogWarning("PublishEventRequest is null");
                return PublishEventResponse.CreateFailure("Request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.EventType))
            {
                _logger.LogWarning("EventType is missing in request");
                return PublishEventResponse.CreateFailure("EventType is required");
            }

            if (string.IsNullOrWhiteSpace(request.EventData))
            {
                _logger.LogWarning("EventData is missing in request");
                return PublishEventResponse.CreateFailure("EventData is required");
            }

            _logger.LogInformation(
                "Processing event publication request - EventType: {EventType}",
                request.EventType);

            // Deserialize event from JSON
            var @event = EventFactory.CreateFromJson(request.EventData);

            if (@event == null)
            {
                _logger.LogWarning(
                    "Failed to deserialize event - EventType: {EventType}",
                    request.EventType);
                return PublishEventResponse.CreateFailure(
                    $"Invalid event format for type: {request.EventType}");
            }

            // Validate event (specific to UserRegisteredEvent or other implementations)
            // UserRegisteredEvent has its own Validate() method
            List<string>? errors = null;
            var eventImpl = @event as dynamic;
            if (eventImpl != null)
            {
                if (!eventImpl.Validate(out errors))
                {
                    var errorMessage = string.Join(", ", errors ?? new List<string>());
                    _logger.LogWarning(
                        "Event validation failed - EventType: {EventType}, Errors: {Errors}",
                        request.EventType,
                        errorMessage);
                    return PublishEventResponse.CreateFailure(
                        $"Event validation failed: {errorMessage}");
                }
            }

            // Get topic for this event type
            var topic = EventTopics.GetTopic(@event.EventType);

            if (string.IsNullOrEmpty(topic))
            {
                _logger.LogWarning(
                    "Unknown event type - EventType: {EventType}",
                    @event.EventType);
                return PublishEventResponse.CreateFailure(
                    $"Unknown event type: {@event.EventType}");
            }

            // Serialize event to JSON for storage
            var eventPayload = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            });

            // Determine partition key (use UserId if available)
            var partitionKey = @event.UserId ?? @event.EventId;

            // Create outbox event (guaranteed delivery)
            var outboxEvent = new OutboxEvent
            {
                EventType = @event.EventType,
                EventPayload = eventPayload,
                Topic = topic,
                PartitionKey = partitionKey,
                IsPublished = false,
                PublishedAt = null,
                OccurredAt = DateTime.UtcNow,
                PublishAttempts = 0
            };

            // Store in outbox
            await _outboxRepository.AddAsync(outboxEvent, cancellationToken);

            _logger.LogInformation(
                "Event stored in outbox - EventId: {EventId}, EventType: {EventType}, Topic: {Topic}",
                @event.EventId,
                @event.EventType,
                topic);

            return PublishEventResponse.CreateSuccess(@event.EventId, @event.CorrelationId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Event publication was cancelled");
            return PublishEventResponse.CreateFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing event - EventType: {EventType}",
                request?.EventType);
            return PublishEventResponse.CreateFailure($"Internal error: {ex.Message}");
        }
    }
}
