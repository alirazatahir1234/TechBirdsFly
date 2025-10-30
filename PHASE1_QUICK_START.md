# ⚡ Phase 1 Quick Start - 90 Minutes to Observability

**Goal**: Get Serilog + Seq + Health Checks running across all 6 services  
**Time**: ~90 minutes  
**Result**: Full request tracing capability  

---

## 📋 Checklist

- [ ] Install Serilog packages (5 min)
- [ ] Update 6 Program.cs files (10 min)
- [ ] Add docker-compose Seq service (2 min)
- [ ] Start docker-compose (2 min)
- [ ] Verify Seq dashboard (3 min)
- [ ] Add exception middleware (10 min)
- [ ] Add health checks (15 min)
- [ ] Test end-to-end (10 min)
- [ ] Add OpenTelemetry (30 min)
- [ ] Verify Jaeger traces (10 min)

**Total: ~90 minutes**

---

## 🚀 Step-by-Step

### Step 1: Install Serilog (5 minutes)

Run in terminal for **each service**:

```bash
# Auth Service
cd services/auth-service/src/AuthService
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context

# Billing Service
cd services/billing-service/src/BillingService
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context

# Generator Service
cd services/generator-service/src/GeneratorService
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context

# Admin Service
cd services/admin-service/src/AdminService
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context

# Image Service
cd services/image-service/src/ImageService
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context

# User Service
cd services/user-service/src/UserService
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context
```

---

### Step 2: Update Program.cs (10 minutes)

For **each service**, add at the **very beginning** of Program.cs:

```csharp
using Serilog;
using Serilog.Context;

// SERILOG CONFIGURATION - MUST BE FIRST
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // CONFIGURE SERILOG
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var serviceName = context.Configuration["SERVICE_NAME"] ?? "AuthService";
        
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(context.Configuration["Serilog:Seq:Url"] ?? "http://seq:80")
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithThreadId();
    });

    // Add middleware for correlation ID
    app.UseMiddleware<CorrelationIdMiddleware>();
    
    // Rest of Program.cs continues...
```

---

### Step 3: Add Correlation ID Middleware (10 minutes)

Create `Middleware/CorrelationIdMiddleware.cs` in **each service**:

```csharp
using Serilog.Context;

namespace AuthService.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var val)
            ? val.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers.Add(CorrelationIdHeader, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
            await _next(context);
            _logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
        }
    }
}
```

---

### Step 4: Add Seq Container (2 minutes)

Add to `infra/docker-compose.yml`:

```yaml
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
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 10s
      timeout: 5s
      retries: 3
```

---

### Step 5: Start Services (2 minutes)

```bash
# From project root
docker-compose -f infra/docker-compose.yml up -d

# Verify Seq is running
docker ps | grep seq
```

---

### Step 6: Verify Logs (3 minutes)

1. Open http://localhost:5341 in browser
2. Make a request to any service:
   ```bash
   curl http://localhost:5001/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com","password":"Test123!"}'
   ```
3. Refresh Seq dashboard
4. You should see logs with **CorrelationId** field

---

### Step 7: Add Exception Middleware (10 minutes)

Create `Middleware/GlobalExceptionMiddleware.cs` in **each service**:

```csharp
using System.Text.Json;

namespace AuthService.Middleware;

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
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            success = false,
            message = exception.Message,
            errorCode = exception.GetType().Name,
            statusCode = context.Response.StatusCode,
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

Add to Program.cs:
```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

---

### Step 8: Add Health Checks (15 minutes)

Add to Program.cs:

```csharp
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!)
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Later in setup
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

Test:
```bash
curl http://localhost:5001/health
curl http://localhost:5001/ready
```

---

### Step 9: Add OpenTelemetry (30 minutes)

Install packages:
```bash
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.Jaeger
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
```

Add to Program.cs:
```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var serviceName = "AuthService"; // Change per service
var resource = ResourceBuilder.CreateDefault()
    .AddService(serviceName, serviceVersion: "1.0.0");

var tracerProvider = new TracerProviderBuilder()
    .SetResourceBuilder(resource)
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddJaegerExporter(options =>
    {
        options.AgentHost = "jaeger";
        options.AgentPort = 6831;
    })
    .Build();

builder.Services.AddSingleton(tracerProvider);
```

---

### Step 10: Add Jaeger Container (2 minutes)

Add to `infra/docker-compose.yml`:

```yaml
  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: techbirdsfly-jaeger
    restart: unless-stopped
    ports:
      - "6831:6831/udp"
      - "16686:16686"
    networks:
      - techbirdsfly_network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:16686/"]
      interval: 10s
      timeout: 5s
      retries: 3
```

Restart services:
```bash
docker-compose -f infra/docker-compose.yml restart
```

---

### Step 11: Test End-to-End (10 minutes)

1. **Make a request**:
   ```bash
   curl http://localhost:5000/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"email":"testuser@example.com","password":"SecurePass123!"}'
   ```

2. **Check Seq** (http://localhost:5341):
   - Click "Events"
   - Filter by "Request"
   - You should see logs with correlation ID

3. **Check Jaeger** (http://localhost:16686):
   - Select "AuthService" from dropdown
   - Click "Find Traces"
   - View the trace showing request processing

---

## ✅ Verification Checklist

- [ ] Serilog packages installed in all 6 services
- [ ] Program.cs updated with Serilog in all 6 services
- [ ] Seq running and accessible at http://localhost:5341
- [ ] Correlation ID middleware in all services
- [ ] Exception middleware in all services
- [ ] Health checks responding at /health and /ready
- [ ] OpenTelemetry configured in all services
- [ ] Jaeger running and accessible at http://localhost:16686
- [ ] Logs appearing in Seq with correlation IDs
- [ ] Traces appearing in Jaeger

---

## 🚨 Troubleshooting

### Seq Dashboard Shows No Logs

**Problem**: Logs not appearing in Seq  
**Solution**:
```bash
# Check Seq is running
docker ps | grep seq

# Check logs
docker logs techbirdsfly-seq

# Verify connection string
# Should be: http://seq:80 (inside Docker network)
```

### Jaeger Shows No Traces

**Problem**: Traces not appearing in Jaeger UI  
**Solution**:
```bash
# Check Jaeger is running
docker ps | grep jaeger

# Agent port must be 6831 (UDP)
# Exporter needs to send to jaeger:6831
```

### Services Won't Start

**Problem**: Compilation error after adding packages  
**Solution**:
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Check for specific errors
dotnet build --verbosity detailed
```

---

## 📊 Expected Results After Phase 1

### Before
```
Service logs scattered
"Request takes 500ms" with no detail
Errors in different formats
No dependency visibility
```

### After Phase 1
```
✅ All logs in Seq dashboard with correlation IDs
✅ Jaeger shows exact bottleneck (e.g., Generator: 200ms)
✅ Standardized error responses
✅ Health checks verify Redis/DB connectivity
✅ Can trace request through entire system
```

---

## 🎯 Next Steps After Phase 1

1. ✅ Phase 1 complete (you are here)
2. 🔜 Phase 2: RabbitMQ Event Integration (Week 2-3)
3. 🔜 Phase 3: Hangfire Background Jobs (Week 3-4)
4. 🔜 Phase 4: Feature Toggles + Notifications (Week 4-6)

---

## 💡 Pro Tips

### Tip 1: Test Specific Service
```bash
# Test only Auth Service without going through gateway
curl http://localhost:5001/health
```

### Tip 2: View Real-Time Logs
```bash
docker logs -f techbirdsfly-auth-service
```

### Tip 3: Search Logs by Correlation ID
In Seq dashboard:
1. Click "Events"
2. In search box: `CorrelationId = "YOUR-ID-HERE"`

### Tip 4: Query Jaeger by Service
1. http://localhost:16686
2. Select service from dropdown
3. Filter by operation name or tags

---

## ⏱️ Time Breakdown

| Task | Time |
|------|------|
| Install packages (6 services) | 5 min |
| Update Program.cs (6 services) | 10 min |
| Add Seq to docker-compose | 2 min |
| Start containers | 2 min |
| Verify Seq dashboard | 3 min |
| Add exception middleware | 10 min |
| Add health checks | 15 min |
| Add OpenTelemetry | 30 min |
| Add Jaeger to docker-compose | 2 min |
| Test end-to-end | 10 min |
| **TOTAL** | **~90 minutes** |

---

**Ready to get started? Begin with Step 1: Install Serilog** ✅

**Need help? Refer to `PHASE1_CODE_TEMPLATES.md` for complete templates**
