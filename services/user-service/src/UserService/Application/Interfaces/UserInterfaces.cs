using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

#region Repository Interfaces

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(List<User> Items, int Total)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfile> AddAsync(UserProfile profile, CancellationToken cancellationToken = default);
    Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Authentication Service Interfaces

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<TokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ResendVerificationEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}

#endregion

#region User Service Interfaces

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<UserListItemDto>> GetUsersAsync(
        ListUsersQuery query,
        CancellationToken cancellationToken = default);

    Task<UserDto> UpdateUserAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateUserAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<bool> ReactivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> SuspendUserAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<bool> AssignRoleAsync(
        Guid userId,
        string role,
        Guid assignedBy,
        CancellationToken cancellationToken = default);

    Task<UserStatisticsDto> GetUserStatisticsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

#endregion

#region Profile Service Interfaces

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateNotificationPreferencesAsync(
        Guid userId,
        bool notificationsEnabled,
        bool emailNotifications,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto> CreateProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

#endregion

#region External Service Interfaces

public interface ITokenService
{
    string GenerateAccessToken(User user, int expirationMinutes = 60);
    string GenerateRefreshToken();
    (Guid UserId, string Username)? ValidateToken(string token);
    string? GetClaimValue(string token, string claimType);
    bool IsTokenExpired(string token);
}

public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IEmailService
{
    Task<bool> SendVerificationEmailAsync(
        string email,
        string username,
        string verificationLink,
        CancellationToken cancellationToken = default);

    Task<bool> SendPasswordResetEmailAsync(
        string email,
        string username,
        string resetLink,
        CancellationToken cancellationToken = default);

    Task<bool> SendWelcomeEmailAsync(
        string email,
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> SendAccountLockedEmailAsync(
        string email,
        string username,
        DateTime unlockTime,
        CancellationToken cancellationToken = default);

    Task<bool> SendRoleAssignmentEmailAsync(
        string email,
        string username,
        string role,
        CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<bool> SendRegistrationNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> SendLoginNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> SendSecurityAlertAsync(
        Guid userId,
        string message,
        CancellationToken cancellationToken = default);

    Task<bool> SendProfileUpdateNotificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

#endregion

#region Event Publisher Interface

public interface IEventPublisher
{
    Task PublishEventAsync<T>(
        T @event,
        CancellationToken cancellationToken = default) where T : class;

    Task PublishEventsAsync<T>(
        IEnumerable<T> events,
        CancellationToken cancellationToken = default) where T : class;
}

#endregion

#region Caching Interface

public interface ICacheService
{
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class;

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default);
}

#endregion
