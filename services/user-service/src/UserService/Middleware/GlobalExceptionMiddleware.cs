using System.Text.Json;

namespace UserService.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<GlobalExceptionMiddleware> logger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred: {ExceptionMessage}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var (statusCode, errorType) = MapExceptionToStatusCode(exception);
            response.StatusCode = statusCode;

            var errorResponse = new ErrorResponse
            {
                Message = exception.Message,
                Type = errorType,
                Timestamp = DateTime.UtcNow
            };

            await response.WriteAsJsonAsync(errorResponse);
        }
    }

    private static (int, string) MapExceptionToStatusCode(Exception exception) =>
        exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "ArgumentError"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NotFound"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "InvalidOperation"),
            _ => (StatusCodes.Status500InternalServerError, "InternalServerError")
        };

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
