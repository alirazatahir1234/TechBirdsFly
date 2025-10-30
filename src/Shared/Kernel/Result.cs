namespace TechBirdsFly.Shared.Kernel;

/// <summary>
/// Generic Result class for handling operation outcomes with success/failure states.
/// Supports both synchronous and asynchronous operations.
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Message { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = new();

    protected Result(bool isSuccess, string message = "", List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? new();
    }

    public static Result Success(string message = "Operation completed successfully")
        => new(true, message);

    public static Result Failure(string message, List<string>? errors = null)
        => new(false, message, errors);

    public static Result Failure(params string[] errors)
        => new(false, "Operation failed", new List<string>(errors));
}

/// <summary>
/// Generic Result class for returning data along with success/failure state.
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; protected set; }

    protected Result(bool isSuccess, T? data = default, string message = "", List<string>? errors = null)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string message = "Operation completed successfully")
        => new(true, data, message);

    public static Result<T> Failure(string message, List<string>? errors = null)
        => new(false, default, message, errors);

    public static Result<T> Failure(params string[] errors)
        => new(false, default, "Operation failed", new List<string>(errors));

    public TResult Match<TResult>(
        Func<T?, TResult> onSuccess,
        Func<string, List<string>, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Data)
            : onFailure(Message, Errors);
    }

    public async Task<TResult> MatchAsync<TResult>(
        Func<T?, Task<TResult>> onSuccess,
        Func<string, List<string>, Task<TResult>> onFailure)
    {
        return IsSuccess
            ? await onSuccess(Data)
            : await onFailure(Message, Errors);
    }
}
