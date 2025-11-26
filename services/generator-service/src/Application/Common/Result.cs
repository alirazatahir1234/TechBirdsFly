namespace GeneratorService.Application.Common;

/// <summary>
/// Generic Result wrapper for operation outcomes
/// Follows the Result pattern for explicit success/failure handling
/// </summary>
/// <typeparam name="T">The data type on success</typeparam>
public class Result<T>
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Data returned on success
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    /// <param name="data">Data to return</param>
    /// <returns>Success result</returns>
    public static Result<T> Ok(T data) => new() { Success = true, Data = data };

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    /// <param name="error">Error message</param>
    /// <returns>Failure result</returns>
    public static Result<T> Fail(string error) => new() { Success = false, Error = error };
}

/// <summary>
/// Non-generic Result wrapper for operations with no return value
/// </summary>
public class Result
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <returns>Success result</returns>
    public static Result Ok() => new() { Success = true };

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    /// <param name="error">Error message</param>
    /// <returns>Failure result</returns>
    public static Result Fail(string error) => new() { Success = false, Error = error };
}
