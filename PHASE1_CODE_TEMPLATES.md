# 🔧 Phase 1 Implementation Templates

This document contains ready-to-use code templates for implementing Phase 1 (Observability Foundation).

Copy and adapt these templates to your services.

---

## Template 1: Serilog Setup (Program.cs)

### Installation

```bash
cd services/auth-service/src/AuthService  # Repeat for each service
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context
dotnet add package Serilog.Enrichers.Environment
```

### Program.cs Configuration

Replace your current `Program.cs` with Serilog setup at the very beginning:

```csharp
using Serilog;
using Serilog.Formatting.Compact;

// ============================================================================
// SERILOG CONFIGURATION - MUST BE FIRST
// ============================================================================

// Create logger for bootstrap errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TechBirdsFly Auth Service");
    
    var builder = WebApplication.CreateBuilder(args);

    // ========================================================================
    // CONFIGURE SERILOG
    // ========================================================================
    
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        // Determine service name from environment or default
        var serviceName = context.Configuration["SERVICE_NAME"] ?? "AuthService";
        
        configuration
            // Set minimum log level from configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            
            // Write to console with nice formatting
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
            
            // Write to Seq (central log aggregation)
            .WriteTo.Seq(
                serverUrl: context.Configuration["Serilog:Seq:Url"] ?? "http://seq:80",
                apiKey: context.Configuration["Serilog:Seq:ApiKey"])
            
            // Add enrichment (context information attached to every log)
            .Enrich.FromLogContext()                          // LogContext.PushProperty
            .Enrich.WithEnvironmentUserName()                 // Username running the service
            .Enrich.WithMachineName()                         // Machine name
            .Enrich.WithProperty("Service", serviceName)      // Service name
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Enrich.WithThreadId()                            // Thread ID for debugging
            
            // Add correlation ID (see middleware below)
            .Enrich.When(
                le => le.Properties.ContainsKey("CorrelationId"),
                e => e.WithProperty("CorrelationId", e.Properties["CorrelationId"]));
    });

    // Rest of Program.cs continues below...
    var app = builder.Build();

    // ========================================================================
    // MIDDLEWARE PIPELINE
    // ========================================================================

    // Add correlation ID middleware FIRST
    app.UseMiddleware<CorrelationIdMiddleware>();
    
    // Then other middleware...
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### appsettings.json Configuration

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "Seq": {
      "Url": "http://seq:80",
      "ApiKey": null
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

### appsettings.Development.json Configuration

```json
{
  "Serilog": {
    "MinimumLevel": "Debug"
  }
}
```

---

## Template 2: Correlation ID Middleware

Create `Middleware/CorrelationIdMiddleware.cs` in each service:

```csharp
namespace AuthService.Middleware;

using Serilog.Context;
using System.Diagnostics;

/// <summary>
/// Middleware to track requests across all microservices using correlation IDs.
/// All requests get a unique ID that flows through the entire system for tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    
    // Standard header name for distributed tracing
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string RequestIdHeader = "X-Request-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create correlation ID
        var correlationId = context.Request.Headers.TryGetValue(
            CorrelationIdHeader, out var headerValue)
            ? headerValue.ToString()
            : Activity.Current?.Id ?? context.TraceIdentifier ?? Guid.NewGuid().ToString();

        // Add to response headers so client can track
        if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
        {
            context.Response.Headers.Add(CorrelationIdHeader, correlationId);
        }

        // Push to Serilog LogContext (attached to all logs in this scope)
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        {
            _logger.LogInformation(
                "Incoming request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            _logger.LogInformation(
                "Response: {StatusCode} {Method} {Path}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path);
        }
    }
}
```

---

## Template 3: Global Exception Middleware

Create `Middleware/GlobalExceptionMiddleware.cs` in each service:

```csharp
namespace AuthService.Middleware;

using System.Text.Json;

/// <summary>
/// Global exception handler that ensures all unhandled exceptions are:
/// 1. Logged with full context
/// 2. Returned in standardized JSON format
/// 3. Do not expose internal implementation details
/// </summary>
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
            _logger.LogError(ex,
                "Unhandled exception in {Path}",
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Determine appropriate HTTP status code
        int statusCode = exception switch
        {
            ArgumentNullException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            Success = false,
            Message = GetUserFriendlyMessage(exception),
            ErrorCode = exception.GetType().Name,
            StatusCode = statusCode,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        // In development, include stack trace
        if (context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }

    /// <summary>
    /// Convert technical exceptions to user-friendly messages
    /// </summary>
    private static string GetUserFriendlyMessage(Exception exception) => exception switch
    {
        ArgumentNullException ex => $"Required field missing: {ex.ParamName}",
        InvalidOperationException => "This operation cannot be performed at this time",
        UnauthorizedAccessException => "You do not have permission to access this resource",
        KeyNotFoundException => "The requested resource was not found",
        _ => "An unexpected error occurred. Please contact support."
    };
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? StackTrace { get; set; } // Only in development
}
```

**Add to Program.cs** (before other middleware):

```csharp
// Add AFTER Serilog middleware, BEFORE routing
app.UseMiddleware<GlobalExceptionMiddleware>();
```

---

## Template 4: Health Check Endpoints

### Add to Program.cs

```csharp
// ============================================================================
// HEALTH CHECKS
// ============================================================================

builder.Services.AddHealthChecks()
    // Check Redis connectivity
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: ["ready", "cache"])
    
    // Check database connectivity (example for PostgreSQL)
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: ["ready", "database"])
    
    // Custom health check for external dependencies
    .AddCheck("external-api", async () =>
    {
        try
        {
            // Example: check if OpenAI API is accessible
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("https://api.openai.com/");
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("OpenAI API unreachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External API check failed", ex);
        }
    }, tags: ["ready"])
    
    // Add more checks as needed
    .AddCheck("startup", () =>
    {
        // Simple check that always passes - used to detect if service is alive
        return HealthCheckResult.Healthy();
    }, tags: ["live"]);

// ... later in app setup

// ============================================================================
// MAP HEALTH CHECK ENDPOINTS
// ============================================================================

// Simple liveness probe (is service running?)
app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResultStatusCodes = new Dictionary<HealthStatus, int>
        {
            { HealthStatus.Healthy, StatusCodes.Status200OK },
            { HealthStatus.Degraded, StatusCodes.Status200OK }, // Still OK
            { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable }
        }
    });

// Readiness probe (is service ready to accept requests?)
app.MapHealthChecks(
    "/ready",
    new HealthCheckOptions
    {
        // Only check items tagged with "ready"
        Predicate = check => check.Tags.Contains("ready"),
        ResultStatusCodes = new Dictionary<HealthStatus, int>
        {
            { HealthStatus.Healthy, StatusCodes.Status200OK },
            { HealthStatus.Degraded, StatusCodes.Status503ServiceUnavailable }, // Not ready
            { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable }
        }
    });
```

### Example Health Check Response

```json
{
  "status": "Healthy",
  "checks": {
    "redis": {
      "status": "Healthy",
      "description": "Connected to Redis",
      "duration": "00:00:00.0234567",
      "data": {}
    },
    "postgresql": {
      "status": "Healthy",
      "description": "PostgreSQL database connection successful",
      "duration": "00:00:00.0987654",
      "data": {}
    },
    "external-api": {
      "status": "Healthy",
      "description": "External API is accessible",
      "duration": "00:00:00.1234567",
      "data": {}
    }
  },
  "totalDuration": "00:00:00.2456788"
}
```

---

## Template 5: OpenTelemetry Configuration

### Installation

```bash
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.Jaeger
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.SqlClient
```

### Program.cs Configuration

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// ============================================================================
// OPENTELEMETRY CONFIGURATION
// ============================================================================

var serviceName = "AuthService"; // Change per service
var serviceVersion = "1.0.0";

var resource = ResourceBuilder.CreateDefault()
    .AddService(serviceName, serviceVersion: serviceVersion)
    .AddAttributes(new Dictionary<string, object>
    {
        { "environment", builder.Environment.EnvironmentName },
        { "service.namespace", "techbirdsfly" }
    });

var tracerProvider = new TracerProviderBuilder()
    .SetResourceBuilder(resource)
    
    // Instrument ASP.NET Core
    .AddAspNetCoreInstrumentation(options =>
    {
        // Record request/response
        options.RecordException = true;
        options.Filter = ctx => !ctx.Request.Path.ToString().Contains("/health");
    })
    
    // Instrument HTTP client calls (for service-to-service)
    .AddHttpClientInstrumentation(options =>
    {
        options.RecordException = true;
        options.EnrichWithHttpRequestMessage = (activity, request) =>
        {
            activity.SetTag("http.request.body", request.Content?.Headers.ContentLength);
        };
    })
    
    // Instrument SQL queries (if using Entity Framework)
    .AddSqlClientInstrumentation(options =>
    {
        options.SetDbStatementForStoredProcedure = true;
        options.RecordException = true;
    })
    
    // Export to Jaeger
    .AddJaegerExporter(options =>
    {
        options.AgentHost = builder.Configuration["Jaeger:AgentHost"] ?? "localhost";
        options.AgentPort = int.Parse(builder.Configuration["Jaeger:AgentPort"] ?? "6831");
        options.MaxPayloadSizeInBytes = 4096;
        options.ExportProcessorType = OpenTelemetry.Sdk.ExportProcessorType.Batch;
    })
    
    .Build();

// Register with DI
builder.Services.AddSingleton(tracerProvider);

// Graceful shutdown
builder.Services.AddHostedService(_ => new TracerProviderService(tracerProvider));

// ... rest of setup

public class TracerProviderService : BackgroundService
{
    private readonly TracerProvider _tracerProvider;

    public TracerProviderService(TracerProvider tracerProvider)
    {
        _tracerProvider = tracerProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _tracerProvider?.ForceFlush();
        _tracerProvider?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
```

### appsettings.json

```json
{
  "Jaeger": {
    "AgentHost": "jaeger",
    "AgentPort": "6831"
  }
}
```

---

## Template 6: Docker Compose Additions

Add these services to `infra/docker-compose.yml`:

```yaml
  # ============================================================================
  # OBSERVABILITY STACK
  # ============================================================================

  seq:
    image: datalust/seq:latest
    container_name: techbirdsfly-seq
    restart: unless-stopped
    ports:
      - "5341:80"
    environment:
      ACCEPT_EULA: "Y"
    networks:
      - techbirdsfly_network
    labels:
      - "com.techbirdsfly.service=observability"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 10s
      timeout: 5s
      retries: 3

  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: techbirdsfly-jaeger
    restart: unless-stopped
    ports:
      - "6831:6831/udp"
      - "16686:16686"
    environment:
      COLLECTOR_OTLP_ENABLED: "true"
    networks:
      - techbirdsfly_network
    labels:
      - "com.techbirdsfly.service=observability"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:16686/"]
      interval: 10s
      timeout: 5s
      retries: 3
```

---

## Template 7: appsettings.json Updates

Add to each service's `appsettings.json`:

```json
{
  "SERVICE_NAME": "AuthService",
  "Serilog": {
    "MinimumLevel": "Information",
    "Seq": {
      "Url": "http://seq:80",
      "ApiKey": null
    }
  },
  "Jaeger": {
    "AgentHost": "jaeger",
    "AgentPort": "6831"
  },
  "HealthChecks": {
    "Enabled": true
  }
}
```

---

## Deployment Checklist

- [ ] Serilog installed in all 6 services
- [ ] Correlation ID middleware in all services
- [ ] Global exception middleware in all services
- [ ] Health check endpoints in all services
- [ ] OpenTelemetry configured in all services
- [ ] Seq container running (`docker-compose up seq`)
- [ ] Jaeger container running (`docker-compose up jaeger`)
- [ ] All services built and running
- [ ] Logs appearing in Seq dashboard (http://localhost:5341)
- [ ] Traces appearing in Jaeger dashboard (http://localhost:16686)

---

## Testing Phase 1

### Test Logging

```bash
# Make a request to any service
curl http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'

# Check Seq dashboard
# http://localhost:5341
# Should see logs for the request with correlation ID
```

### Test Health Checks

```bash
# Liveness probe
curl http://localhost:5001/health
# Response: 200 OK with detailed health status

# Readiness probe
curl http://localhost:5001/ready
# Response: 200 OK only if all dependencies (Redis, DB) are healthy
```

### Test Distributed Tracing

```bash
# Make a request through the gateway
curl http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'

# Check Jaeger dashboard
# http://localhost:16686
# Select "AuthService" from service dropdown
# View the trace showing all spans (request processing details)
```

### Test Exception Handling

```bash
# Make a bad request
curl http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"invalid"}'

# Check response format
# Should return standardized error with correlation ID
```

---

## Next Steps

1. ✅ Complete Phase 1 implementation across all 6 services
2. 🔜 Move to Phase 2: RabbitMQ Event-Driven Architecture
3. 🔜 Move to Phase 3: Hangfire Background Jobs
4. 🔜 Move to Phase 4: Feature Toggles & Notifications

---

**Use these templates as-is in your services. Adjust service names and configurations as needed.**

**Questions? Refer to `ENTERPRISE_ARCHITECTURE_ASSESSMENT.md` and `PHASE1_IMPLEMENTATION_ROADMAP.md`**
