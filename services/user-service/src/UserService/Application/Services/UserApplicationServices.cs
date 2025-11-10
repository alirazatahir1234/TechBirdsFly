using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Events;

namespace UserService.Application.Services;

#region Authentication Service

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IEmailService _emailService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHashService passwordHashService,
        IEmailService emailService,
        IEventPublisher eventPublisher,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Password != request.ConfirmPassword)
                return new AuthResponse(false, "Passwords do not match", null, null, null, null);

            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
                return new AuthResponse(false, "Email already registered", null, null, null, null);

            var existingUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUsername != null)
                return new AuthResponse(false, "Username already taken", null, null, null, null);

            var passwordHash = _passwordHashService.HashPassword(request.Password);
            var email = new EmailAddress(request.Email);
            var phone = request.Phone != null ? new PhoneNumber(request.Phone) : null;

            var userResult = User.Create(
                request.Username,
                email,
                passwordHash,
                request.FullName,
                phone);

            if (!userResult.IsSuccess || userResult.Data == null)
                return new AuthResponse(false, userResult.Error, null, null, null, null);

            var addedUser = await _userRepository.AddAsync(userResult.Data, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // Publish registration event
            var registeredEvent = new UserRegisteredEvent
            {
                AggregateId = addedUser.Id,
                Username = addedUser.Username,
                Email = addedUser.Email.Value,
                FullName = addedUser.FullName
            };
            await _eventPublisher.PublishEventAsync(registeredEvent, cancellationToken);

            // Send verification email
            var verificationLink = $"https://techbirdsfly.com/verify-email?userId={addedUser.Id}&code=temp";
            await _emailService.SendVerificationEmailAsync(addedUser.Email.Value, addedUser.Username, verificationLink, cancellationToken);

            var userDto = MapToUserDto(addedUser);
            var token = _tokenService.GenerateAccessToken(addedUser);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("User {Username} registered successfully", request.Username);

            return new AuthResponse(true, "Registration successful. Please verify your email.", userDto, token, refreshToken, DateTime.UtcNow.AddHours(1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Username}", request.Username);
            return new AuthResponse(false, "Registration failed", null, null, null, null);
        }
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (user == null)
                return new AuthResponse(false, "Invalid username or password", null, null, null, null);

            if (!user.IsActive())
                return new AuthResponse(false, "Account is not active", null, null, null, null);

            if (!_passwordHashService.VerifyPassword(request.Password, user.PasswordHash))
            {
                user.RecordFailedLoginAttempt();
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);

                if (user.IsLockedOut())
                {
                    await _emailService.SendAccountLockedEmailAsync(
                        user.Email.Value,
                        user.Username,
                        user.LockoutUntil!.Value,
                        cancellationToken);
                }

                return new AuthResponse(false, "Invalid username or password", null, null, null, null);
            }

            // Successful login
            user.RecordSuccessfulLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // Publish login event
            var loginEvent = new UserLoggedInEvent
            {
                AggregateId = user.Id,
                Username = user.Username,
                LoginTime = DateTime.UtcNow
            };
            await _eventPublisher.PublishEventAsync(loginEvent, cancellationToken);

            var userDto = MapToUserDto(user);
            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return new AuthResponse(true, "Login successful", userDto, token, refreshToken, DateTime.UtcNow.AddHours(1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user {Username}", request.Username);
            return new AuthResponse(false, "Login failed", null, null, null, null);
        }
    }

    public async Task<TokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Refresh token functionality to be implemented with token store");
    }

    public async Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.FromResult(_tokenService.ValidateToken(token) != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    public async Task<AuthResponse> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            throw new NotImplementedException("Email verification with token store to be implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return new AuthResponse(false, "Email verification failed", null, null, null, null);
        }
    }

    public async Task<bool> ResendVerificationEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
                return false;

            var verificationLink = $"https://techbirdsfly.com/verify-email?userId={user.Id}&code=temp";
            return await _emailService.SendVerificationEmailAsync(user.Email.Value, user.Username, verificationLink, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email");
            return false;
        }
    }

    public async Task<bool> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                return true; // Return true to prevent email enumeration

            var resetLink = $"https://techbirdsfly.com/reset-password?userId={user.Id}&code=temp";
            return await _emailService.SendPasswordResetEmailAsync(user.Email.Value, user.Username, resetLink, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password");
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            throw new NotImplementedException("Password reset with token store to be implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.NewPassword != request.ConfirmPassword)
                return false;

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            if (!_passwordHashService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return false;

            var newPasswordHash = _passwordHashService.HashPassword(request.NewPassword);
            user.ChangePassword(newPasswordHash);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // Publish password changed event
            var passwordChangedEvent = new PasswordChangedEvent
            {
                AggregateId = user.Id,
                ChangedAt = DateTime.UtcNow
            };
            await _eventPublisher.PublishEventAsync(passwordChangedEvent, cancellationToken);

            _logger.LogInformation("User {UserId} changed password", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return false;
        }
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("User {UserId} logged out", userId);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out user {UserId}", userId);
        }
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email.Value,
            user.FullName,
            user.Phone?.Value,
            user.Role.ToString(),
            user.Status.ToString(),
            user.EmailVerified,
            user.ProfileImageUrl,
            user.Bio,
            user.CreatedAt,
            user.UpdatedAt,
            user.LastLoginAt);
    }
}

#endregion

#region User Service

public class UserApplicationService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IProfileService _profileService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserApplicationService> _logger;

    public UserApplicationService(
        IUserRepository userRepository,
        IProfileService profileService,
        IEventPublisher eventPublisher,
        IEmailService emailService,
        ILogger<UserApplicationService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return null;
        }
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username {Username}", username);
            return null;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return null;
        }
    }

    public async Task<PaginatedResponse<UserListItemDto>> GetUsersAsync(
        ListUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (users, total) = await _userRepository.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SortBy,
                query.Ascending,
                cancellationToken);

            var items = users
                .Select(u => new UserListItemDto(u.Id, u.Username, u.Email.Value, u.FullName, u.Role.ToString(), u.Status.ToString(), u.CreatedAt))
                .ToList();

            var totalPages = (int)Math.Ceiling(total / (double)query.PageSize);

            return new PaginatedResponse<UserListItemDto>(items, total, query.PageNumber, query.PageSize, totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated users");
            return new PaginatedResponse<UserListItemDto>(new List<UserListItemDto>(), 0, 1, 20, 0);
        }
    }

    public async Task<UserDto> UpdateUserAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.UpdateProfile(
                request.FullName ?? user.FullName,
                request.Phone != null ? new PhoneNumber(request.Phone) : user.Phone,
                request.Bio ?? user.Bio,
                request.ProfileImageUrl ?? user.ProfileImageUrl);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // Publish profile updated event
            var profileUpdatedEvent = new ProfileUpdatedEvent
            {
                AggregateId = user.Id,
                UpdatedFields = "FullName, Phone, Bio, ProfileImageUrl",
                UpdatedAt = DateTime.UtcNow
            };
            await _eventPublisher.PublishEventAsync(profileUpdatedEvent, cancellationToken);

            _logger.LogInformation("User {UserId} profile updated", userId);
            return MapToUserDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeactivateUserAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.Deactivate();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var deactivatedEvent = new UserDeactivatedEvent
            {
                AggregateId = user.Id,
                Reason = reason ?? "User account deactivation",
                DeactivatedAt = DateTime.UtcNow
            };
            await _eventPublisher.PublishEventAsync(deactivatedEvent, cancellationToken);

            _logger.LogInformation("User {UserId} deactivated", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ReactivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.Reactivate();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} reactivated", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SuspendUserAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            user.Suspend();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} suspended", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> AssignRoleAsync(
        Guid userId,
        string role,
        Guid assignedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            if (!Enum.TryParse<UserRole>(role, out var userRole))
                return false;

            user.AssignRole(userRole);
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var roleAssignedEvent = new RoleAssignedEvent
            {
                AggregateId = user.Id,
                Role = role,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow
            };
            await _eventPublisher.PublishEventAsync(roleAssignedEvent, cancellationToken);

            await _emailService.SendRoleAssignmentEmailAsync(user.Email.Value, user.Username, role, cancellationToken);

            _logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserId}", userId);
            return false;
        }
    }

    public async Task<UserStatisticsDto> GetUserStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var userList = allUsers.ToList();

            return new UserStatisticsDto(
                userList.Count,
                userList.Count(u => u.IsActive()),
                userList.Count(u => u.Status == UserStatus.Suspended),
                userList.Count(u => u.IsLockedOut()),
                userList.Count(u => u.Role == UserRole.Admin),
                userList.Count(u => u.CreatedAt.Date == DateTime.UtcNow.Date));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return new UserStatisticsDto(0, 0, 0, 0, 0, 0);
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userRepository.DeleteAsync(userId, cancellationToken);
            if (result)
            {
                await _userRepository.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("User {UserId} deleted", userId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return false;
        }
    }

    private UserDto MapToUserDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email.Value,
            user.FullName,
            user.Phone?.Value,
            user.Role.ToString(),
            user.Status.ToString(),
            user.EmailVerified,
            user.ProfileImageUrl,
            user.Bio,
            user.CreatedAt,
            user.UpdatedAt,
            user.LastLoginAt);
    }
}

#endregion

#region Profile Service

public class ProfileService : IProfileService
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        IUserProfileRepository profileRepository,
        IUserRepository userRepository,
        IEventPublisher eventPublisher,
        ILogger<ProfileService> logger)
    {
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserProfileDto> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
            if (profile == null)
            {
                // Create default profile if not exists
                return await CreateProfileAsync(userId, cancellationToken);
            }

            return MapToProfileDto(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            profile.CompanyName = request.CompanyName;
            profile.Department = request.Department;
            profile.JobTitle = request.JobTitle;
            profile.Location = request.Location;
            profile.Website = request.Website;
            profile.UpdatedAt = DateTime.UtcNow;

            var updated = profile.Id == Guid.Empty
                ? await _profileRepository.AddAsync(profile, cancellationToken)
                : await _profileRepository.UpdateAsync(profile, cancellationToken);

            await _profileRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile updated for user {UserId}", userId);
            return MapToProfileDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateNotificationPreferencesAsync(
        Guid userId,
        bool notificationsEnabled,
        bool emailNotifications,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
            if (profile == null)
                return false;

            profile.NotificationsEnabled = notificationsEnabled;
            profile.EmailNotifications = emailNotifications;
            profile.UpdatedAt = DateTime.UtcNow;

            await _profileRepository.UpdateAsync(profile, cancellationToken);
            await _profileRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Notification preferences updated for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences for user {UserId}", userId);
            return false;
        }
    }

    public async Task<UserProfileDto> CreateProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationsEnabled = true,
                EmailNotifications = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _profileRepository.AddAsync(profile, cancellationToken);
            await _profileRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profile created for user {UserId}", userId);
            return MapToProfileDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating profile for user {UserId}", userId);
            throw;
        }
    }

    private UserProfileDto MapToProfileDto(UserProfile profile)
    {
        return new UserProfileDto(
            profile.Id,
            profile.UserId,
            profile.CompanyName,
            profile.Department,
            profile.JobTitle,
            profile.Location,
            profile.Website,
            profile.NotificationsEnabled,
            profile.EmailNotifications);
    }
}

#endregion
