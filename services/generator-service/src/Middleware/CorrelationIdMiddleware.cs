using Serilog.Context;

namespace GeneratorService.Middleware;

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
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        using (LogContext.PushProperty(CorrelationIdProperty, correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }
        var newCorrelationId = Guid.NewGuid().ToString("N");
        context.Items[CorrelationIdProperty] = newCorrelationId;
        return newCorrelationId;
    }
}
