using System;
using System.Collections.Generic;
using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

#region Auth DTOs

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName,
    string? Phone = null);

public record LoginRequest(
    string Username,
    string Password,
    bool RememberMe = false);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);

public record ForgotPasswordRequest(
    string Email);

public record VerifyEmailRequest(
    string Token);

#endregion

#region User DTOs

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string? Phone,
    string Role,
    string Status,
    bool EmailVerified,
    string? ProfileImageUrl,
    string? Bio,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt);

public record UserListItemDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string Status,
    DateTime CreatedAt);

public record UserProfileDto(
    Guid Id,
    Guid UserId,
    string? CompanyName,
    string? Department,
    string? JobTitle,
    string? Location,
    string? Website,
    bool NotificationsEnabled,
    bool EmailNotifications);

public record UpdateProfileRequest(
    string? FullName = null,
    string? Phone = null,
    string? Bio = null,
    string? ProfileImageUrl = null,
    string? CompanyName = null,
    string? Department = null,
    string? JobTitle = null,
    string? Location = null,
    string? Website = null);

#endregion

#region Auth Response DTOs

public record AuthResponse(
    bool Success,
    string? Message,
    UserDto? User,
    string? Token,
    string? RefreshToken,
    DateTime? ExpiresAt);

public record TokenResponse(
    string AccessToken,
    string? RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer");

#endregion

#region Admin DTOs

public record AssignRoleRequest(
    Guid UserId,
    string Role);

public record SuspendUserRequest(
    Guid UserId,
    string? Reason = null);

public record UserStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int SuspendedUsers,
    int LockedUsers,
    int AdminUsers,
    int RegisteredToday);

#endregion

#region Pagination & Generic DTOs

public record ListUsersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool Ascending = true,
    string? FilterByRole = null,
    string? FilterByStatus = null,
    string? SearchTerm = null);

public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages)
{
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null,
    List<string>? Errors = null);

public record ApiResponse(
    bool Success,
    string? Message = null,
    List<string>? Errors = null);

#endregion

#region User Search & Filter DTOs

public record UserSearchResult(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    DateTime CreatedAt);

public record UserFilterOptions(
    List<string>? Roles = null,
    List<string>? Statuses = null,
    DateTime? CreatedAfter = null,
    DateTime? CreatedBefore = null);

#endregion
