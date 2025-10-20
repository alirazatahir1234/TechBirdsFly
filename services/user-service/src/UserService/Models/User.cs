namespace UserService.Models;

/// <summary>
/// User entity representing a registered user
/// </summary>
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string? Website { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    
    public string Status { get; set; } = "active"; // active, inactive, suspended, deleted
    public string Role { get; set; } = "user"; // user, admin, moderator
    
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string? EmailVerificationToken { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public int LoginCount { get; set; }
    public int ProjectCount { get; set; }
    public int ImageGenerationCount { get; set; }
    
    public string? TimeZone { get; set; }
    public string? PreferredLanguage { get; set; } = "en";
    public Dictionary<string, string>? Preferences { get; set; }
}

/// <summary>
/// User profile details
/// </summary>
public class UserProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
    public string DisplayName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Occupation { get; set; }
    public string? Interests { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User preferences and settings
/// </summary>
public class UserPreference
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
    public bool NotifyEmail { get; set; } = true;
    public bool NotifyPush { get; set; } = true;
    public bool NotifyNewsletter { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    
    public string? Theme { get; set; } = "light";
    public string? Language { get; set; } = "en";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User subscription/plan information
/// </summary>
public class UserSubscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
    public string PlanType { get; set; } = "free"; // free, starter, pro, enterprise
    public string Status { get; set; } = "active"; // active, paused, cancelled, expired
    
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    
    public decimal MonthlyCost { get; set; }
    public int MonthlyImageGenerations { get; set; }
    public int MonthlyStorageGb { get; set; }
    
    public int UsedGenerations { get; set; }
    public decimal UsedStorageGb { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ============================================================================
// REQUEST/RESPONSE DTOs
// ============================================================================

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string? Website { get; set; }
    public string? Bio { get; set; }
    public string? TimeZone { get; set; }
    public string? PreferredLanguage { get; set; }
}

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginCount { get; set; }
    public int ProjectCount { get; set; }
    public int ImageGenerationCount { get; set; }
}

public class UserListResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<UserResponse> Users { get; set; } = new();
}
