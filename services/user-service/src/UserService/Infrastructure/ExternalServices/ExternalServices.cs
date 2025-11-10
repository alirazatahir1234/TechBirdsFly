using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.ExternalServices;

#region Token Service

public class TokenService : ITokenService
{
    private readonly string _jwtSecret;
    private readonly ILogger<TokenService> _logger;

    public TokenService(string jwtSecret, ILogger<TokenService> logger)
    {
        if (string.IsNullOrEmpty(jwtSecret))
            throw new ArgumentNullException(nameof(jwtSecret));

        _jwtSecret = jwtSecret;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GenerateAccessToken(User user, int expirationMinutes = 60)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email.Value),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Status", user.Status.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = "techbirdsfly",
                Audience = "techbirdsfly-users",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token for user {UserId}", user.Id);
            throw;
        }
    }

    public string GenerateRefreshToken()
    {
        try
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token");
            throw;
        }
    }

    public (Guid UserId, string Username)? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "techbirdsfly",
                ValidateAudience = true,
                ValidAudience = "techbirdsfly-users",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

            if (userIdClaim == null || usernameClaim == null)
                return null;

            if (Guid.TryParse(userIdClaim.Value, out var userId))
                return (userId, usernameClaim.Value);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed");
            return null;
        }
    }

    public string? GetClaimValue(string token, string claimType)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            return jwtToken?.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting claim {ClaimType} from token", claimType);
            return null;
        }
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            return jwtToken?.ValidTo <= DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token expiration");
            return true;
        }
    }
}

#endregion

#region Password Hash Service

public class PasswordHashService : IPasswordHashService
{
    private readonly ILogger<PasswordHashService> _logger;

    public PasswordHashService(ILogger<PasswordHashService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string HashPassword(string password)
    {
        try
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }
}

#endregion

#region Email Service

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendVerificationEmailAsync(
        string email,
        string username,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending verification email to {Email} for user {Username}", email, username);

            // TODO: Implement email sending with SendGrid, SMTP, or similar service
            // For now, just log and return success
            _logger.LogInformation("Verification email sent successfully to {Email}", email);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(
        string email,
        string username,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending password reset email to {Email} for user {Username}", email, username);

            // TODO: Implement email sending with SendGrid, SMTP, or similar service
            // For now, just log and return success
            _logger.LogInformation("Password reset email sent successfully to {Email}", email);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(
        string email,
        string username,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending welcome email to {Email} for user {Username}", email, username);

            // TODO: Implement email sending with SendGrid, SMTP, or similar service
            // For now, just log and return success
            _logger.LogInformation("Welcome email sent successfully to {Email}", email);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendAccountLockedEmailAsync(
        string email,
        string username,
        DateTime unlockTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending account locked notification to {Email} for user {Username}", email, username);

            // TODO: Implement email sending with SendGrid, SMTP, or similar service
            // For now, just log and return success
            _logger.LogInformation("Account locked notification sent successfully to {Email}", email);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending account locked notification to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendRoleAssignmentEmailAsync(
        string email,
        string username,
        string role,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending role assignment notification to {Email} for user {Username}", email, username);

            // TODO: Implement email sending with SendGrid, SMTP, or similar service
            // For now, just log and return success
            _logger.LogInformation("Role assignment notification sent successfully to {Email}", email);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending role assignment notification to {Email}", email);
            return false;
        }
    }
}

#endregion

#region Notification Service

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IEmailService emailService, ILogger<NotificationService> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendRegistrationNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending registration notification for user {UserId}", userId);
            // TODO: Implement notification sending logic
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending registration notification for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SendLoginNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending login notification for user {UserId}", userId);
            // TODO: Implement notification sending logic
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending login notification for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SendSecurityAlertAsync(
        Guid userId,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending security alert for user {UserId}: {Message}", userId, message);
            // TODO: Implement notification sending logic
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security alert for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SendProfileUpdateNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending profile update notification for user {UserId}", userId);
            // TODO: Implement notification sending logic
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending profile update notification for user {UserId}", userId);
            return false;
        }
    }
}

#endregion

#region Event Publisher

public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(ILogger<EventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishEventAsync<T>(
        T @event,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _logger.LogInformation("Publishing event of type {EventType}", typeof(T).Name);

            // TODO: Implement event publishing to message bus (RabbitMQ, Azure Service Bus, etc.)
            // For now, just log the event
            _logger.LogInformation("Event published: {@Event}", @event);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event of type {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task PublishEventsAsync<T>(
        IEnumerable<T> events,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            foreach (var @event in events)
            {
                await PublishEventAsync(@event, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing events");
            throw;
        }
    }
}

#endregion

#region Cache Service

public class CacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime? Expiration)> _cache = new();
    private readonly ILogger<CacheService> _logger;
    private readonly object _lockObject = new();

    public CacheService(ILogger<CacheService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            lock (_lockObject)
            {
                if (_cache.TryGetValue(key, out var cached))
                {
                    if (cached.Expiration.HasValue && cached.Expiration.Value <= DateTime.UtcNow)
                    {
                        _cache.Remove(key);
                        return null;
                    }

                    return cached.Value as T;
                }
            }

            return await Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            lock (_lockObject)
            {
                var expirationTime = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;
                _cache[key] = (value, expirationTime);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key {Key}", key);
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lockObject)
            {
                _cache.Remove(key);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_lockObject)
            {
                if (!_cache.TryGetValue(key, out var cached))
                    return false;

                if (cached.Expiration.HasValue && cached.Expiration.Value <= DateTime.UtcNow)
                {
                    _cache.Remove(key);
                    return false;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key {Key}", key);
            return false;
        }
    }
}

#endregion
