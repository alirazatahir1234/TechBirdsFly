using System.Net;
using System.Text.Json;
using Serilog;

namespace GeneratorService.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            error = new
            {
                message = GetErrorMessage(exception),
                type = exception.GetType().Name,
                timestamp = DateTime.UtcNow
            }
        };
        context.Response.StatusCode = GetStatusCode(exception);
        return context.Response.WriteAsJsonAsync(response);
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static string GetErrorMessage(Exception exception) =>
        exception switch
        {
            ArgumentNullException argNull => $"Required argument is null: {argNull.ParamName}",
            ArgumentException arg => $"Invalid argument: {arg.Message}",
            InvalidOperationException => "Operation is not valid in current context",
            UnauthorizedAccessException => "Access denied",
            KeyNotFoundException => "Resource not found",
            _ => "An internal server error occurred"
        };
}
