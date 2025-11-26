using Microsoft.AspNetCore.Http;
using Serilog;

namespace GeneratorService.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches unhandled exceptions and returns appropriate HTTP responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Unhandled exception occurred in request path {Path}", context.Request.Path);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                error = "An unexpected error occurred",
                message = exception.Message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
