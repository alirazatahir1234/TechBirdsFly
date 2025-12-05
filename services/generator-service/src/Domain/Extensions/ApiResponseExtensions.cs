namespace GeneratorService.WebAPI.Extensions;

/// <summary>
/// Extension methods for standardized API responses
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Wraps data in standardized API response format
    /// </summary>
    public static ApiResponse<T> ToApiResponse<T>(this T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a success response with message
    /// </summary>
    public static ApiResponse<T> ToApiResponse<T>(this T data, string message)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiErrorResponse ToErrorResponse(this string error)
    {
        return new ApiErrorResponse
        {
            Success = false,
            Error = error,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Standard success response format
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Standard error response format
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public object? Error { get; set; }
    public DateTime Timestamp { get; set; }
}
