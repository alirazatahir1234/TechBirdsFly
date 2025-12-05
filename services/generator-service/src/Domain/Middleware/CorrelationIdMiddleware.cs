using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace GeneratorService.Middleware;

/// <summary>
/// Middleware for adding correlation ID to all requests and responses
/// Enables request tracking across the system
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request header or generate new one
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdValue)
            ? correlationIdValue.ToString()
            : Activity.Current?.Id ?? context.TraceIdentifier;

        // Set correlation ID in response header
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Continue processing
        await _next(context);
    }
}
