using UserService.Models;
using UserService.Data;
using Microsoft.EntityFrameworkCore;

namespace UserService.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public interface IUserManagementService
{
    Task<UserResponse?> GetUserByIdAsync(string userId);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserListResponse> GetUsersAsync(int page = 1, int pageSize = 20);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> UpdateLastLoginAsync(string userId);
    Task<bool> VerifyEmailAsync(string userId, string token);
}

public class UserManagementService : IUserManagementService
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(UserDbContext context, ILogger<UserManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserResponse?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }

            return MapToUserResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null;
            }

            return MapToUserResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<UserListResponse> GetUsersAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Users.Where(u => u.Status != "deleted");
            var total = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new UserListResponse
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Users = users.Select(MapToUserResponse).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users list");
            throw;
        }
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if user already exists
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existing != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Company = request.Company,
                Status = "active",
                Role = "user",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            
            // Create default profile
            var profile = new UserProfile
            {
                UserId = user.Id,
                DisplayName = $"{user.FirstName} {user.LastName}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Profiles.Add(profile);

            // Create default preferences
            var preferences = new UserPreference
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Preferences.Add(preferences);

            // Create default subscription (free plan)
            var subscription = new UserSubscription
            {
                UserId = user.Id,
                PlanType = "free",
                Status = "active",
                MonthlyCost = 0m,
                MonthlyImageGenerations = 10,
                MonthlyStorageGb = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Subscriptions.Add(subscription);

            await _context.SaveChangesAsync();

            _logger.LogInformation("User created: {UserId} ({Email})", user.Id, user.Email);
            return MapToUserResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User not found: {userId}");
            }

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;
            
            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;
            
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;
            
            if (!string.IsNullOrEmpty(request.Company))
                user.Company = request.Company;
            
            if (!string.IsNullOrEmpty(request.Website))
                user.Website = request.Website;
            
            if (!string.IsNullOrEmpty(request.Bio))
                user.Bio = request.Bio;
            
            if (!string.IsNullOrEmpty(request.TimeZone))
                user.TimeZone = request.TimeZone;
            
            if (!string.IsNullOrEmpty(request.PreferredLanguage))
                user.PreferredLanguage = request.PreferredLanguage;

            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated: {UserId}", userId);
            return MapToUserResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            user.Status = "deleted";
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.LoginCount++;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> VerifyEmailAsync(string userId, string token)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            if (user.EmailVerificationToken != token)
            {
                return false;
            }

            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email: {UserId}", userId);
            throw;
        }
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Status = user.Status,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            LoginCount = user.LoginCount,
            ProjectCount = user.ProjectCount,
            ImageGenerationCount = user.ImageGenerationCount
        };
    }
}
