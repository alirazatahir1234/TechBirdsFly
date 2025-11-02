using TechBirdsFly.Shared.Events.Contracts;
using AuthService.Application.Interfaces;

namespace AuthService.Application.Services;

/// <summary>
/// Service for publishing authentication-related events
/// Integrates with Event Bus to publish UserRegistered and other user events
/// </summary>
public class AuthEventPublisherService
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuthEventPublisherService> _logger;

    public AuthEventPublisherService(
        IEventPublisher eventPublisher,
        ILogger<AuthEventPublisherService> logger)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publish UserRegistered event when a new user is registered
    /// </summary>
    public async Task PublishUserRegisteredEventAsync(
        string userId,
        string email,
        string firstName,
        string lastName,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "📤 Publishing UserRegistered event - UserId: {UserId}, Email: {Email}",
                userId,
                email);

            // Create event
            var @event = UserRegisteredEvent.Create(
                userId: userId,
                email: email,
                firstName: firstName,
                lastName: lastName,
                correlationId: correlationId);

            // Publish to Event Bus
            await _eventPublisher.PublishEventAsync(@event, cancellationToken);

            _logger.LogInformation(
                "✅ UserRegistered event published successfully - EventId: {EventId}, UserId: {UserId}",
                @event.EventId,
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Failed to publish UserRegistered event - UserId: {UserId}, Error: {Error}",
                userId,
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Publish UserUpdated event when user profile is updated
    /// </summary>
    public async Task PublishUserUpdatedEventAsync(
        string userId,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "📤 Publishing UserUpdated event - UserId: {UserId}",
                userId);

            // TODO: Create and publish UserUpdated event
            // When UserUpdated event is created in shared contracts

            _logger.LogInformation(
                "✅ UserUpdated event published - UserId: {UserId}",
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Failed to publish UserUpdated event - UserId: {UserId}",
                userId);
            throw;
        }
    }

    /// <summary>
    /// Publish UserDeactivated event when user account is deactivated
    /// </summary>
    public async Task PublishUserDeactivatedEventAsync(
        string userId,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "📤 Publishing UserDeactivated event - UserId: {UserId}",
                userId);

            // TODO: Create and publish UserDeactivated event
            // When UserDeactivated event is created in shared contracts

            _logger.LogInformation(
                "✅ UserDeactivated event published - UserId: {UserId}",
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Failed to publish UserDeactivated event - UserId: {UserId}",
                userId);
            throw;
        }
    }
}
