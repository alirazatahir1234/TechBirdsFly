using System.Net;
using FluentValidation;
using Newtonsoft.Json;

namespace GeneratorService.WebAPI.Middleware;

/// <summary>
/// Global error handling middleware
/// Catches all exceptions and returns standardized error responses
/// </summary>
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning($"Validation error: {string.Join(", ", ex.Errors.Select(e => e.ErrorMessage))}");
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Errors.Select(e => e.ErrorMessage).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unhandled exception: {ex.Message}");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, new[] { ex.Message });
        }
    }

    private async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, object? message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var error = new
        {
            success = false,
            statusCode = (int)statusCode,
            error = message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
    }
}
