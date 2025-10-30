using Serilog.Context;

namespace AuthService.Middleware;

/// <summary>
/// Middleware that adds a correlation ID to every request for distributed tracing.
/// Enables request tracking across multiple services in microservices architecture.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdProperty = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add to response headers using indexer
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        // Add to Serilog context for all logs within this request
        using (LogContext.PushProperty(CorrelationIdProperty, correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // Check if correlation ID exists in request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }

        // Create new correlation ID if not present
        var newCorrelationId = Guid.NewGuid().ToString("N");
        context.Items[CorrelationIdProperty] = newCorrelationId;
        return newCorrelationId;
    }
}
