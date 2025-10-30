# 🗺️ TechBirdsFly Phase-Based Implementation Roadmap

**Current State**: Foundation Complete (Microservices + Caching + Gateway)  
**Goal**: Production-Enterprise Architecture  
**Timeline**: 4-6 weeks to completion

---

## 📍 Overview Map

```
┌─────────────────────────────────────────────────────────────┐
│                    ENTERPRISE ARCHITECTURE                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Phase 1: Observability Foundation (Week 1-2)             │
│  ├─ Serilog + Seq (centralized logging)                    │
│  ├─ GlobalExceptionMiddleware (standardized errors)        │
│  ├─ OpenTelemetry + Jaeger (distributed tracing)          │
│  └─ Health Check Endpoints (/health, /ready)              │
│                                                             │
│  Phase 2: Async Communication (Week 2-3)                  │
│  ├─ RabbitMQ Integration                                   │
│  ├─ Event-Driven Architecture                              │
│  ├─ Message Producers & Consumers                          │
│  └─ Dead-Letter Queue Handling                             │
│                                                             │
│  Phase 3: Resilience & Jobs (Week 3-4)                    │
│  ├─ Hangfire Background Jobs                               │
│  ├─ Scheduled Tasks                                        │
│  ├─ Circuit Breaker Patterns                               │
│  └─ Retry Policies                                         │
│                                                             │
│  Phase 4: Operations & Scale (Week 4-6)                   │
│  ├─ Feature Toggle Service                                 │
│  ├─ Notification Service                                   │
│  ├─ Metrics Dashboard (Prometheus + Grafana)               │
│  └─ Request Correlation                                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

# 🔴 PHASE 1: Observability Foundation (Week 1-2)

**Objective**: Add **logging**, **error handling**, **tracing**, and **health checks** across all services.

## Deliverables

### 1. Serilog + Seq Centralized Logging

**What**: Replace console-only logging with structured logging aggregation.

**Why**: 
- Cannot debug microservice interactions with console logs alone
- Need persistent logs for auditing and compliance
- Need to correlate requests across service boundaries

**Impact**:
- Visibility into all service operations
- Historical log retention
- Cross-service request tracing

**Implementation Steps**:

```bash
# Step 1: Add NuGet packages to each service
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Context

# Step 2: Update Program.cs in each service
# (See template below)

# Step 3: Add Seq container to docker-compose.yml
# (See container config below)

# Step 4: Verify logs flowing to Seq dashboard
# Open http://localhost:5341 after docker-compose up
```

**Docker Compose Addition**:
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

**Program.cs Template**:
```csharp
// Serilog setup
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.Seq("http://seq:80")
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "AuthService");
});

// Rest of program...
```

**Effort**: 1-2 hours (parallel across all 6 services)

---

### 2. Global Exception Middleware

**What**: Standardize all error responses across all services.

**Why**:
- Inconsistent error formats make frontend integration painful
- Unhandled exceptions expose internal details
- Need standardized error codes for API clients

**Current Problem**:
```csharp
// Service A
return BadRequest("Invalid email");

// Service B
return BadRequest(new { error = "Invalid email", code = "INVALID_EMAIL" });

// Service C
throw new Exception("Invalid email"); // Unhandled, crashes
```

**Solution - Unified Response**:
```json
{
  "success": false,
  "message": "Invalid email format",
  "errorCode": "INVALID_EMAIL",
  "statusCode": 400,
  "traceId": "0HN1GCMK8VS1K:00000001",
  "timestamp": "2025-10-29T10:30:45Z"
}
```

**Implementation**:

1. Create shared `GlobalExceptionMiddleware.cs`:
```csharp
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

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = ex.Message,
            errorCode = ex.GetType().Name,
            statusCode = context.Response.StatusCode,
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(response);
    }
}
```

2. Register in Program.cs:
```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

**Effort**: 1-2 hours (create once, apply to all 6 services)

---

### 3. Health Check Endpoints

**What**: Add `/health` and `/ready` endpoints to all services.

**Why**:
- Required for Kubernetes deployment (liveness & readiness probes)
- Container orchestration needs to know service health
- Enables graceful shutdown and dependency checking

**Implementation**:

```csharp
// Add to Program.cs
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!)
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddCheck("custom-check", () => HealthCheckResult.Healthy());

// Map endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

**Response Format**:
```json
{
  "status": "Healthy",
  "checks": {
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.1234567"
    },
    "postgresql": {
      "status": "Healthy",
      "duration": "00:00:00.0987654"
    }
  },
  "totalDuration": "00:00:00.2222222"
}
```

**Effort**: 1-2 hours (all 6 services in parallel)

---

### 4. OpenTelemetry + Jaeger Distributed Tracing

**What**: Trace requests across all microservices to identify bottlenecks.

**Why**:
- Cannot debug cross-service issues with logs alone
- Need to see latency breakdown between services
- Essential for performance optimization

**How It Works**:
```
User Request
  ↓
Gateway (span: 5ms)
  ├─ Auth Service (span: 50ms)
  │   └─ Database call (span: 45ms)
  ├─ Generator Service (span: 200ms)
  │   ├─ OpenAI API (span: 150ms)
  │   └─ Database call (span: 45ms)
  └─ Image Service (span: 150ms)
      └─ Database call (span: 140ms)

Total: 405ms (visible in Jaeger dashboard with full breakdown)
```

**Implementation**:

1. Add NuGet packages:
```bash
dotnet add package OpenTelemetry.Exporter.Jaeger
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.SqlClient
```

2. Add to Program.cs:
```csharp
var tracerProvider = new TracerProviderBuilder()
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddJaegerExporter(options =>
    {
        options.AgentHost = "jaeger";
        options.AgentPort = 6831;
    })
    .Build();
```

3. Add Jaeger container to docker-compose.yml:
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
```

**Dashboard**: Access at `http://localhost:16686`

**Effort**: 2-3 hours (includes Jaeger setup and service instrumentation)

---

## Phase 1 Checklist

- [ ] Serilog added to all 6 services
- [ ] Seq container running and receiving logs
- [ ] GlobalExceptionMiddleware implemented in all services
- [ ] /health and /ready endpoints on all services
- [ ] OpenTelemetry instrumentation on all services
- [ ] Jaeger container running and displaying traces
- [ ] Test end-to-end request flow with tracing

---

# 🟡 PHASE 2: Async Communication (Week 2-3)

**Objective**: Enable real event-driven communication between services via RabbitMQ.

## Current Problem

```csharp
// Current: LocalMessagePublisher (in-process only)
await _publisher.PublishAsync(new GenerateWebsiteJobRequest { /* ... */ });

// Problem:
// - Message only stays in memory
// - If service crashes, message is lost
// - Other services can't subscribe
// - No retry mechanism
```

## Solution: RabbitMQ Integration

### 1. Core Event Bus Setup

**Create IEventBus Interface**:
```csharp
public interface IEventBus
{
    Task PublishAsync<T>(T message, string? routingKey = null) where T : IEvent;
    Task SubscribeAsync<T>(Func<T, Task> handler) where T : IEvent;
}

public interface IEvent
{
    string EventType { get; }
    DateTime Timestamp { get; }
}
```

### 2. RabbitMQ Implementation

**Create RabbitMQEventBus**:
```csharp
public class RabbitMQEventBus : IEventBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventBus> _logger;

    public async Task PublishAsync<T>(T message, string? routingKey = null) where T : IEvent
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: "techbirdsfly-exchange",
                routingKey: routingKey ?? message.EventType,
                basicProperties: properties,
                body: body);

            _logger.LogInformation($"Event published: {message.EventType}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event");
            throw;
        }
    }

    public Task SubscribeAsync<T>(Func<T, Task> handler) where T : IEvent
    {
        // Consumer setup...
        return Task.CompletedTask;
    }
}
```

### 3. Service Integration Example

**In Generator Service**:
```csharp
// Publish event when project is generated
await _eventBus.PublishAsync(new ProjectGeneratedEvent
{
    ProjectId = project.Id,
    UserId = userId,
    WebsiteUrl = downloadUrl,
    Timestamp = DateTime.UtcNow
});

// Other services subscribe:
// BillingService: Track usage
// AdminService: Log analytics
// NotificationService: Send email
```

### 4. Registration & Configuration

**Program.cs**:
```csharp
builder.Services.AddScoped<IEventBus, RabbitMQEventBus>();
builder.Services.AddScoped<RabbitMQConnectionFactory>();

// Configure connection
builder.Services.Configure<RabbitMQOptions>(
    builder.Configuration.GetSection("RabbitMQ"));
```

**appsettings.json**:
```json
{
  "RabbitMQ": {
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

### 5. Consumer Pattern (Background Service)

**Create EventConsumer**:
```csharp
public class ProjectGeneratedEventConsumer : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProjectGeneratedEventConsumer> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventBus.SubscribeAsync<ProjectGeneratedEvent>(
            async e => await HandleProjectGenerated(e));
    }

    private async Task HandleProjectGenerated(ProjectGeneratedEvent e)
    {
        _logger.LogInformation($"Received: {e.ProjectId} generated");
        // Handle event (update billing, send notification, etc.)
    }
}
```

### 6. Dead-Letter Queue (DLQ)

**Why**: Messages that fail multiple times need somewhere to go.

```csharp
// Setup DLQ
_channel.QueueDeclare(
    queue: "project-generated-dlq",
    durable: true,
    exclusive: false);

// Main queue with DLQ routing
var args = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "dlx" },
    { "x-dead-letter-routing-key", "project-generated-dlq" }
};

_channel.QueueDeclare(
    queue: "project-generated",
    durable: true,
    exclusive: false,
    arguments: args);
```

## Phase 2 Deliverables

- [ ] IEventBus interface created
- [ ] RabbitMQ client implementation
- [ ] Event producer/consumer pattern
- [ ] Dead-letter queue configuration
- [ ] Integration tested end-to-end
- [ ] Documentation for team

**Effort**: 3-4 hours

---

# 🟡 PHASE 3: Background Jobs & Resilience (Week 3-4)

**Objective**: Add scheduled/background job processing with Hangfire.

## Use Cases

1. **Generate Invoices** (1st of month): Query usage data, create invoice PDFs, send via email
2. **Send Reminders**: Check for overdue invoices, send email reminders
3. **Cache Cleanup**: Expire old sessions, clean abandoned projects
4. **Metrics Aggregation**: Calculate daily/monthly metrics
5. **Report Generation**: Generate usage reports for admins

## Hangfire Setup

**1. Add NuGet Package**:
```bash
dotnet add package Hangfire.Core
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.Redis.StackExchange
```

**2. Program.cs Configuration**:
```csharp
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseRedisStorage(builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddHangfireServer();

// ... in app setup
app.UseHangfireDashboard("/hangfire");
```

**3. Create Background Job Service**:
```csharp
public interface IBackgroundJobService
{
    string ScheduleGenerateInvoices();
    string ScheduleCleanupSessions();
    string ScheduleMetricsAggregation();
}

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;

    public string ScheduleGenerateInvoices()
    {
        return RecurringJob.AddOrUpdate<InvoiceGenerator>(
            "generate-invoices",
            x => x.GenerateMonthlyInvoicesAsync(),
            Cron.Monthly(1, 0)); // 1st of month at midnight
    }

    public string ScheduleCleanupSessions()
    {
        return RecurringJob.AddOrUpdate<SessionCleaner>(
            "cleanup-sessions",
            x => x.CleanExpiredSessionsAsync(),
            Cron.Daily(3, 0)); // 3 AM daily
    }

    public string ScheduleMetricsAggregation()
    {
        return RecurringJob.AddOrUpdate<MetricsAggregator>(
            "aggregate-metrics",
            x => x.AggregateAsync(),
            Cron.Hourly); // Every hour
    }
}
```

**4. Dashboard**: Access at `http://localhost:5001/hangfire`

**Effort**: 3-4 hours

---

# 🟢 PHASE 4: Operations & Scale (Week 4-6)

**Objective**: Add operational features and scale-ready patterns.

## 4.1 Feature Toggle Service

**Use Case**: 
- Roll out new features gradually
- A/B testing without deployment
- Kill switches for problematic features
- Per-user feature access

**Simple Implementation** (In-Memory with Redis):
```csharp
public interface IFeatureToggleService
{
    Task<bool> IsEnabledAsync(string featureName, string? userId = null);
    Task SetAsync(string featureName, bool enabled);
}

public class RedisFeatureToggleService : IFeatureToggleService
{
    private readonly IDistributedCache _cache;

    public async Task<bool> IsEnabledAsync(string featureName, string? userId = null)
    {
        var key = userId != null 
            ? $"feature:{featureName}:{userId}" 
            : $"feature:{featureName}:global";

        var value = await _cache.GetStringAsync(key);
        return value == "true";
    }
}
```

## 4.2 Notification Service

**Create new service** at `/services/notification-service/`

**Responsibilities**:
- Subscribe to RabbitMQ events
- Send emails via SendGrid
- Send SMS via Twilio
- Track delivery status

## 4.3 Metrics Dashboard (Prometheus + Grafana)

**Add to docker-compose.yml**:
```yaml
prometheus:
  image: prom/prometheus:latest
  container_name: techbirdsfly-prometheus
  ports:
    - "9090:9090"
  networks:
    - techbirdsfly_network

grafana:
  image: grafana/grafana:latest
  container_name: techbirdsfly-grafana
  ports:
    - "3001:3000"
  networks:
    - techbirdsfly_network
```

**Effort**: 4-5 hours

---

## 📊 Timeline Summary

| Phase | Weeks | Focus | Status |
|-------|-------|-------|--------|
| **Phase 1** | 1-2 | Observability (Logging, Tracing, Health Checks) | 🔴 Next |
| **Phase 2** | 2-3 | Async Communication (RabbitMQ Events) | 🟡 After Phase 1 |
| **Phase 3** | 3-4 | Background Jobs (Hangfire) | 🟡 After Phase 2 |
| **Phase 4** | 4-6 | Operations (Toggles, Notifications, Metrics) | 🟢 After Phase 3 |

---

## 🎯 Success Criteria

### Phase 1
- [x] All services logging to Seq
- [x] Errors have standardized format
- [x] Jaeger dashboard showing request traces
- [x] /health endpoint responds from all services

### Phase 2
- [x] Events flowing through RabbitMQ
- [x] Services consuming events asynchronously
- [x] Dead-letter queue handling failures
- [x] No messages lost on service restart

### Phase 3
- [x] Hangfire dashboard accessible
- [x] Recurring jobs running on schedule
- [x] Job logs visible in Seq
- [x] Retry policies working

### Phase 4
- [x] Feature toggles working without redeployment
- [x] Notification service operational
- [x] Grafana dashboard showing metrics
- [x] Request correlation visible in logs

---

## 💾 Deployment Considerations

### Development
```bash
docker-compose -f infra/docker-compose.yml up -d
```

### Staging/Production
- Use Azure Container Registry for images
- Azure Service Bus instead of RabbitMQ (optional)
- Azure Application Insights for monitoring
- Azure Key Vault for secrets

---

## 📞 Quick Reference

| Component | Port | URL | Purpose |
|-----------|------|-----|---------|
| Seq | 5341 | http://localhost:5341 | Log aggregation UI |
| Jaeger | 16686 | http://localhost:16686 | Tracing UI |
| Hangfire | 5001 | http://localhost:5001/hangfire | Job dashboard |
| RabbitMQ | 15672 | http://localhost:15672 | Message queue UI (guest/guest) |
| Grafana | 3001 | http://localhost:3001 | Metrics dashboard |

---

**Ready to start Phase 1? I'll create implementation templates next!** 🚀
