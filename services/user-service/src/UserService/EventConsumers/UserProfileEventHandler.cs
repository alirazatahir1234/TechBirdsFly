using TechBirdsFly.Shared.Events.Contracts;
using UserService.Services;
using Serilog.Context;

namespace UserService.EventConsumers;

/// <summary>
/// Event handler for UserRegistered events
/// Creates user profiles when new users register
/// </summary>
public class UserProfileEventHandler
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UserProfileEventHandler> _logger;

    public UserProfileEventHandler(
        IUserManagementService userManagementService,
        ILogger<UserProfileEventHandler> logger)
    {
        _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle UserRegistered event from Event Bus
    /// Creates a new user profile in the User Service database
    /// </summary>
    public async Task HandleUserRegisteredAsync(
        UserRegisteredEvent @event,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Add correlation ID to logging context for tracing
            using (LogContext.PushProperty("CorrelationId", @event.CorrelationId))
            using (LogContext.PushProperty("EventId", @event.EventId))
            {
                _logger.LogInformation(
                    "📨 Received UserRegistered event - UserId: {UserId}, Email: {Email}",
                    @event.UserId,
                    @event.Email);

                // Check if user already exists (idempotency)
                try
                {
                    var existingUser = await _userManagementService.GetUserByIdAsync(@event.UserId);
                    if (existingUser != null)
                    {
                        _logger.LogInformation(
                            "ℹ️ User profile already exists, skipping creation - UserId: {UserId}",
                            @event.UserId);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "User lookup check (may not exist yet) - UserId: {UserId}", @event.UserId);
                }

                // Create user profile from event data
                var createUserRequest = new Models.CreateUserRequest
                {
                    Email = @event.Email,
                    FirstName = @event.FirstName,
                    LastName = @event.LastName
                };

                _logger.LogInformation(
                    "🔄 Creating user profile from event - UserId: {UserId}, Email: {Email}",
                    @event.UserId,
                    @event.Email);

                // Create the user profile
                var userResponse = await _userManagementService.CreateUserAsync(createUserRequest);

                // Initialize default subscription (free plan)
                _logger.LogInformation(
                    "🎁 Initializing free subscription for new user - UserId: {UserId}",
                    @event.UserId);

                // Here you could call subscription initialization
                // For now, this is logged as a TODO

                _logger.LogInformation(
                    "✅ User profile created successfully from event - UserId: {UserId}, Email: {Email}",
                    @event.UserId,
                    @event.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Error processing UserRegistered event - EventId: {EventId}, UserId: {UserId}",
                @event.EventId,
                @event.UserId);

            // Re-throw to mark event as failed in the consumer
            // This allows the Event Bus to retry or send to a DLQ (Dead Letter Queue)
            throw;
        }
    }
}
