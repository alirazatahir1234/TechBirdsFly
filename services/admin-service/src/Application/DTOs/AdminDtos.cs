namespace AdminService.Application.DTOs;

/// <summary>
/// DTO for Admin User responses
/// </summary>
public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }
    public int ProjectCount { get; set; }
    public decimal TotalSpent { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
}

/// <summary>
/// DTO for creating a new admin user
/// </summary>
public class CreateAdminUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an admin user
/// </summary>
public class UpdateAdminUserRequest
{
    public string? FullName { get; set; }
    public int? ProjectCount { get; set; }
    public decimal? TotalSpent { get; set; }
}

/// <summary>
/// DTO for suspending an admin user
/// </summary>
public class SuspendAdminUserRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Role responses
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new role
/// </summary>
public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// DTO for updating a role
/// </summary>
public class UpdateRoleRequest
{
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// DTO for Audit Log responses
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? AdminUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for querying audit logs with filters
/// </summary>
public class AuditLogFilterRequest
{
    public Guid? AdminUserId { get; set; }
    public string? Action { get; set; }
    public string? ResourceType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Generic Response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new()
        };
    }
}
