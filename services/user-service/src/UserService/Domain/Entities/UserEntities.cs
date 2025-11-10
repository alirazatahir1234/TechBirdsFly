using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

/// <summary>
/// User role enumeration
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1,
    Moderator = 2,
    Support = 3
}

/// <summary>
/// User status enumeration
/// </summary>
public enum UserStatus
{
    Pending,
    Active,
    Suspended,
    Deactivated,
    Locked
}

/// <summary>
/// Email address value object
/// </summary>
public record EmailAddress(string Value)
{
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Value) &&
        Value.Contains("@") &&
        Value.Length >= 5;

    public override string ToString() => Value;
}

/// <summary>
/// Phone number value object
/// </summary>
public record PhoneNumber(string? Value)
{
    public bool IsValid() =>
        string.IsNullOrEmpty(Value) || (Value.Length >= 10 && Value.All(char.IsDigit));
}

/// <summary>
/// Result helper for domain operations
/// </summary>
public record Result<T>(bool IsSuccess, T? Data, string? Error)
{
    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

/// <summary>
/// User aggregate root
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = new EmailAddress(string.Empty);
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public PhoneNumber? Phone { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public bool EmailVerified { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string? Bio { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int LoginAttempts { get; private set; }
    public DateTime? LockoutUntil { get; private set; }

    public User() { }

    /// <summary>
    /// Create new user
    /// </summary>
    public static Result<User> Create(
        string username,
        EmailAddress email,
        string passwordHash,
        string fullName,
        PhoneNumber? phone = null)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return Result<User>.Failure("Username must be at least 3 characters");

        if (!email.IsValid())
            return Result<User>.Failure("Invalid email address");

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result<User>.Failure("Password hash is required");

        if (string.IsNullOrWhiteSpace(fullName))
            return Result<User>.Failure("Full name is required");

        if (phone != null && !phone.IsValid())
            return Result<User>.Failure("Invalid phone number");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username.ToLowerInvariant(),
            Email = email,
            PasswordHash = passwordHash,
            FullName = fullName,
            Phone = phone,
            Role = UserRole.User,
            Status = UserStatus.Pending,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        return Result<User>.Success(user);
    }

    /// <summary>
    /// Verify user email
    /// </summary>
    public void VerifyEmail()
    {
        if (EmailVerified)
            throw new InvalidOperationException("Email is already verified");

        EmailVerified = true;
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update profile information
    /// </summary>
    public void UpdateProfile(string fullName, PhoneNumber? phone, string? bio, string? profileImageUrl)
    {
        FullName = fullName ?? FullName;
        Phone = phone ?? Phone;
        Bio = bio ?? Bio;
        ProfileImageUrl = profileImageUrl ?? ProfileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Change password
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty");

        PasswordHash = newPasswordHash;
        LoginAttempts = 0;
        LockoutUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record successful login
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LoginAttempts = 0;
        LockoutUntil = null;
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record failed login attempt
    /// </summary>
    public void RecordFailedLoginAttempt(int maxAttempts = 5, int lockoutMinutes = 30)
    {
        LoginAttempts++;

        if (LoginAttempts >= maxAttempts)
        {
            Status = UserStatus.Locked;
            LockoutUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unlock user account
    /// </summary>
    public void Unlock()
    {
        if (Status != UserStatus.Locked)
            throw new InvalidOperationException("User is not locked");

        Status = UserStatus.Active;
        LoginAttempts = 0;
        LockoutUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    public void AssignRole(UserRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Suspend user account
    /// </summary>
    public void Suspend()
    {
        if (Status == UserStatus.Suspended)
            throw new InvalidOperationException("User is already suspended");

        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate suspended user
    /// </summary>
    public void Reactivate()
    {
        if (Status != UserStatus.Suspended)
            throw new InvalidOperationException("User is not suspended");

        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Deactivated)
            throw new InvalidOperationException("User is already deactivated");

        Status = UserStatus.Deactivated;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if user is locked out
    /// </summary>
    public bool IsLockedOut()
    {
        if (Status != UserStatus.Locked || !LockoutUntil.HasValue)
            return false;

        if (DateTime.UtcNow > LockoutUntil.Value)
        {
            Unlock();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if account is active
    /// </summary>
    public bool IsActive() =>
        Status == UserStatus.Active &&
        EmailVerified &&
        !IsLockedOut();
}

/// <summary>
/// User profile entity
/// </summary>
public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? SocialMediaLinks { get; set; }
    public string? Preferences { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool EmailNotifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserProfile() { }

    public UserProfile(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        NotificationsEnabled = true;
        EmailNotifications = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(
        string? companyName,
        string? department,
        string? jobTitle,
        string? location,
        string? website,
        string? preferences)
    {
        CompanyName = companyName ?? CompanyName;
        Department = department ?? Department;
        JobTitle = jobTitle ?? JobTitle;
        Location = location ?? Location;
        Website = website ?? Website;
        Preferences = preferences ?? Preferences;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotificationPreferences(bool notifications, bool emailNotifications)
    {
        NotificationsEnabled = notifications;
        EmailNotifications = emailNotifications;
        UpdatedAt = DateTime.UtcNow;
    }
}
