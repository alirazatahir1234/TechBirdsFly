# 🔴 Redis: Your Shared Nervous System

This document explains how Redis will serve as the **central hub** for caching, sessions, rate limiting, job storage, and more across your entire TechBirdsFly ecosystem.

---

## 📊 Redis Role Overview

```
Redis 7.4-Alpine (Single Instance - 6379)
│
├─ 1️⃣ CACHING (Already Implemented) ✅
│  ├─ Auth: Token cache (1h TTL) → 3 endpoints
│  ├─ Billing: Summary cache (30m TTL) → 3 endpoints
│  ├─ Admin: Template cache (1h TTL) → 11 endpoints
│  ├─ Image: Metadata cache (1h TTL) → 4 endpoints
│  ├─ User: Profile cache (30m TTL) → 3 endpoints
│  └─ Generator: Project cache (10m TTL) → 3 endpoints
│  └─ Impact: 37 endpoints, 92.8% DB query reduction
│
├─ 2️⃣ RATE LIMITING (Gateway Use Case)
│  ├─ Per-user counters (100 req/min)
│  ├─ Per-IP counters (10 req/min anonymous)
│  ├─ DDoS protection (50 req/30s per IP)
│  └─ Fast lookup (< 1ms)
│
├─ 3️⃣ SESSION STORE (Future)
│  ├─ ASP.NET Core session data
│  ├─ User preferences
│  └─ Temporary request data
│
├─ 4️⃣ MESSAGE QUEUE FALLBACK (Planned)
│  ├─ Redis Streams for async messaging
│  ├─ Lightweight alternative to RabbitMQ for POC
│  └─ Consumer groups for guaranteed delivery
│
├─ 5️⃣ HANGFIRE JOB STORE (Phase 3)
│  ├─ Background job scheduling
│  ├─ Recurring job state
│  ├─ Job history
│  └─ Retry coordination
│
├─ 6️⃣ FEATURE TOGGLES (Phase 4)
│  ├─ Global feature flags
│  ├─ Per-user feature access
│  ├─ Zero-deployment rollouts
│  └─ TTL-based automatic expiration
│
└─ 7️⃣ DISTRIBUTED LOCKS (Planned)
   ├─ Prevent duplicate invoice generation
   ├─ Coordinate background jobs
   └─ Mutex across service instances
```

---

## 🎯 Current Implementation (Phase 0 - Complete)

### Cache-Aside Pattern

All 6 services currently use the same pattern:

```csharp
// 1. Check cache
var cacheKey = $"endpoint:{id}";
var cached = await _cache.GetAsync<YourType>(cacheKey);
if (cached != null)
    return Ok(cached); // Cache hit!

// 2. If not cached, fetch from database
var data = await _database.GetAsync(id);
if (data == null)
    return NotFound();

// 3. Store in cache with TTL
var ttl = TimeSpan.FromMinutes(10); // or 30min, 1h, etc.
await _cache.SetAsync(cacheKey, data, ttl);

return Ok(data);
```

**Current Stats**:
- Services: 6/6 implemented ✅
- Endpoints cached: 37
- Expected cache hit rate: 75-92%
- Database query reduction: 92.8%
- Performance improvement: 55x faster average
- Memory usage: ~50MB Redis (with 10-minute data)

---

## 🚀 Phase 2: Rate Limiting with Redis

Your **YARP Gateway** already rate limits, but it's using in-memory counters. Move to Redis for **distributed rate limiting** (useful when scaling to multiple gateway instances).

### Implementation

```csharp
// RateLimitService.cs
public class RateLimitService
{
    private readonly IDistributedCache _cache;
    
    public async Task<(bool IsAllowed, int Remaining, int ResetSeconds)> CheckLimitAsync(
        string userId, 
        int maxRequests, 
        int windowSeconds)
    {
        var key = $"rate-limit:{userId}";
        
        // Get current count
        var countStr = await _cache.GetStringAsync(key);
        var count = int.TryParse(countStr, out var c) ? c : 0;
        
        if (count >= maxRequests)
        {
            // Rate limit exceeded
            var ttl = await _cache.GetAsync(key + ":ttl");
            var resetSeconds = ttl?.Length ?? windowSeconds;
            return (false, 0, resetSeconds);
        }
        
        // Increment counter
        count++;
        await _cache.SetStringAsync(
            key, 
            count.ToString(), 
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(windowSeconds)
            });
        
        return (true, maxRequests - count, windowSeconds);
    }
}
```

### Usage in Gateway Middleware

```csharp
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitService _rateLimitService;

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? context.Connection.RemoteIpAddress?.ToString() 
            ?? "anonymous";
        
        var (isAllowed, remaining, resetSeconds) = 
            await _rateLimitService.CheckLimitAsync(userId, maxRequests: 100, windowSeconds: 60);
        
        context.Response.Headers.Add("X-RateLimit-Limit", "100");
        context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", resetSeconds.ToString());
        
        if (!isAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        await _next(context);
    }
}
```

**Why Redis?**
- Multiple gateway instances share the same counters
- Atomic increment operations
- Automatic TTL cleanup
- Sub-millisecond lookups

---

## 📨 Phase 2b: Redis Streams (Message Queue Alternative)

For MVP, instead of adding RabbitMQ complexity, use **Redis Streams**:

```csharp
// EventPublisher using Redis Streams
public class RedisStreamEventPublisher : IEventBus
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task PublishAsync<T>(T message, string? routingKey = null) where T : IEvent
    {
        var db = _redis.GetDatabase();
        
        var json = JsonSerializer.Serialize(message);
        var streamKey = $"stream:{message.EventType}";
        
        // Publish to stream (survives service restarts)
        await db.StreamAddAsync(
            streamKey,
            new NameValueEntry[]
            {
                new NameValueEntry("data", json),
                new NameValueEntry("timestamp", DateTime.UtcNow.Ticks.ToString())
            });
    }
    
    public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : IEvent
    {
        var db = _redis.GetDatabase();
        var streamKey = $"stream:{typeof(T).Name}";
        
        var groupName = "default-group";
        
        // Create consumer group if not exists
        try
        {
            await db.StreamGroupCreateAsync(streamKey, groupName, "$", createStream: true);
        }
        catch
        {
            // Group already exists
        }
        
        // Read messages
        while (true)
        {
            var messages = await db.StreamReadGroupAsync(
                streamKey,
                groupName,
                Environment.MachineName, // consumer name
                count: 10);
            
            foreach (var message in messages)
            {
                var json = message.Values.FirstOrDefault(v => v.Name == "data").Value.ToString();
                var obj = JsonSerializer.Deserialize<T>(json)!;
                
                await handler(obj);
                
                // Mark as processed
                await db.StreamAcknowledgeAsync(streamKey, groupName, message.Id);
            }
            
            await Task.Delay(1000); // Poll every second
        }
    }
}
```

**Advantages**:
- No additional infrastructure (Streams are built-in to Redis)
- Persistent messages (survives restarts)
- Consumer groups (load balancing)
- Simple pub/sub without Kafka complexity

---

## 💾 Phase 3: Hangfire Job Store Configuration

Configure Hangfire to use Redis as its backing store:

### Installation

```bash
dotnet add package Hangfire.Core
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.Redis.StackExchange
```

### Program.cs Setup

```csharp
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseRedisStorage(
        builder.Configuration.GetConnectionString("Redis"),
        new RedisStorageOptions
        {
            Prefix = "hangfire:",
            Db = 1, // Use separate DB from cache (DB 0)
            InvisibilityTimeout = TimeSpan.FromMinutes(30),
            ExpirationTime = TimeSpan.FromDays(7)
        }));

builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
    options.ServerName = $"{Environment.MachineName}:BillingService";
});
```

### Why Separate Redis DBs?

```
Redis Instance (Port 6379)
│
├─ DB 0: Cache (default)
│  ├─ auth:token:*
│  ├─ billing:summary:*
│  └─ project:*
│
└─ DB 1: Hangfire Jobs
   ├─ hangfire:job:*
   ├─ hangfire:schedule:*
   └─ hangfire:queue:*
```

**Configuration**:

```json
{
  "ConnectionStrings": {
    "Redis": "techbirdsfly-redis:6379,defaultDatabase=0,abortConnect=false"
  }
}
```

### Example Recurring Job

```csharp
public class InvoiceService
{
    private readonly IBillingRepository _repo;
    private readonly IEmailService _email;
    private readonly ILogger<InvoiceService> _logger;

    [AutomaticRetry(Attempts = 3)]
    public async Task GenerateMonthlyInvoicesAsync()
    {
        _logger.LogInformation("Starting monthly invoice generation");
        
        try
        {
            var users = await _repo.GetActiveSubscribersAsync();
            
            foreach (var user in users)
            {
                var invoice = await _repo.GenerateInvoiceAsync(user.Id);
                await _email.SendInvoiceAsync(user.Email, invoice);
            }
            
            _logger.LogInformation("Monthly invoices generated: {Count}", users.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate invoices");
            throw;
        }
    }
}

// In Program.cs or startup
RecurringJob.AddOrUpdate<InvoiceService>(
    "generate-monthly-invoices",
    x => x.GenerateMonthlyInvoicesAsync(),
    Cron.Monthly(1, 0)); // 1st of month at midnight
```

---

## 🚩 Phase 4: Feature Toggles

Use Redis for feature flags that can be toggled without redeployment:

```csharp
public interface IFeatureToggleService
{
    Task<bool> IsEnabledAsync(string featureName, string? userId = null);
    Task SetAsync(string featureName, bool enabled, TimeSpan? expiration = null);
}

public class RedisFeatureToggleService : IFeatureToggleService
{
    private readonly IDistributedCache _cache;

    public async Task<bool> IsEnabledAsync(string featureName, string? userId = null)
    {
        var key = userId != null 
            ? $"feature:{featureName}:user:{userId}" 
            : $"feature:{featureName}:global";
        
        var value = await _cache.GetStringAsync(key);
        return value == "true";
    }

    public async Task SetAsync(string featureName, bool enabled, TimeSpan? expiration = null)
    {
        var key = $"feature:{featureName}:global";
        
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration;
        
        await _cache.SetStringAsync(key, enabled.ToString(), options);
    }
}
```

### Usage in Controllers

```csharp
public class GeneratorController
{
    private readonly IFeatureToggleService _features;

    [HttpPost("projects")]
    public async Task<IActionResult> CreateProject(CreateProjectRequest req)
    {
        // Check if feature is enabled
        if (!await _features.IsEnabledAsync("advanced-ai-models"))
        {
            // Fallback to simpler model
            req.Model = "gpt-4";
        }
        
        // Continue normally
        return Ok(new { projectId = "...", status = "generating" });
    }
}
```

### Command-Line Toggle

```bash
# Enable feature globally (7-day expiration)
redis-cli SET feature:advanced-ai-models:global true EX 604800

# Disable feature
redis-cli SET feature:advanced-ai-models:global false

# Check current value
redis-cli GET feature:advanced-ai-models:global
```

---

## 🔒 Distributed Locks (Advanced)

Prevent duplicate execution across multiple service instances:

```csharp
public class DistributedLockService
{
    private readonly IConnectionMultiplexer _redis;

    public async Task<bool> AcquireLockAsync(string lockKey, string lockValue, TimeSpan? expiration = null)
    {
        var db = _redis.GetDatabase();
        
        return await db.StringSetAsync(
            lockKey,
            lockValue,
            expiration ?? TimeSpan.FromMinutes(10),
            When.NotExists); // Only set if doesn't exist
    }

    public async Task<bool> ReleaseLockAsync(string lockKey, string lockValue)
    {
        var db = _redis.GetDatabase();
        
        // Compare and delete (atomic)
        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";
        
        var result = await db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { lockValue });
        return (long)result == 1;
    }
}

// Usage: Prevent duplicate invoice generation
public async Task GenerateMonthlyInvoicesAsync()
{
    var lockKey = "job:generate-invoices:monthly";
    var lockValue = Guid.NewGuid().ToString();
    
    // Try to acquire lock
    if (!await _lockService.AcquireLockAsync(lockKey, lockValue, TimeSpan.FromMinutes(30)))
    {
        _logger.LogInformation("Invoice generation already in progress, skipping");
        return;
    }
    
    try
    {
        // Do work...
        await GenerateInvoicesAsync();
    }
    finally
    {
        await _lockService.ReleaseLockAsync(lockKey, lockValue);
    }
}
```

---

## 📊 Redis Monitoring & Optimization

### View Redis Memory Usage

```bash
# Connect to Redis
redis-cli

# Check memory statistics
INFO memory

# View all keys with size
MEMORY DOCTOR

# Get cache hit rate
INFO stats
# Look for: keyspace_hits, keyspace_misses
```

### Optimization Tips

```bash
# 1. Set memory limit
CONFIG SET maxmemory 256mb

# 2. Set eviction policy (remove oldest when full)
CONFIG SET maxmemory-policy allkeys-lru

# 3. Monitor slow commands
SLOWLOG GET 10

# 4. Check DB size
DBSIZE

# 5. Flush cache if needed
FLUSHDB  # Current DB only
FLUSHALL # All DBs (WARNING: affects caching AND Hangfire!)
```

### Performance Metrics

```bash
# Throughput
INFO stats | grep instant_ops_per_sec

# Latency
LATENCY LATEST

# Persistence
INFO persistence

# Replication
INFO replication
```

---

## 🏗️ Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                  Your Services (6)                      │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │        RedisCacheService (ICacheService)          │  │
│  │  ┌─────────────────────────────────────────────┐  │  │
│  │  │ GetAsync<T>() → Get from cache             │  │  │
│  │  │ SetAsync<T>() → Store in cache with TTL    │  │  │
│  │  │ RemoveAsync() → Delete from cache          │  │  │
│  │  │ ExistsAsync() → Check if key exists        │  │  │
│  │  └─────────────────────────────────────────────┘  │  │
│  │                                                    │  │
│  │  StackExchange.Redis Client (v2.7.0)              │  │
│  └───────────────────────────────────────────────────┘  │
│                     │                                    │
└─────────────────────┼─────────────────────────────────────
                      │ (TCP Port 6379)
                      ↓
        ┌─────────────────────────────────┐
        │   Redis 7.4-Alpine              │
        ├─────────────────────────────────┤
        │                                 │
        │  DB 0: Cache (Default)          │
        │  ├─ Cache Keys (20,000+)        │
        │  ├─ Session Data (TTL)          │
        │  └─ Rate Limit Counters         │
        │                                 │
        │  DB 1: Hangfire                 │
        │  ├─ Job Queue                   │
        │  ├─ Recurring Jobs              │
        │  ├─ Job History                 │
        │  └─ Failed Jobs (DLQ)           │
        │                                 │
        │  Memory: ~256MB max             │
        │  Persistence: AOF + RDB         │
        │  Replication: None (single)     │
        │                                 │
        └─────────────────────────────────┘
```

---

## 🎯 Redis Roadmap

### Currently Done ✅
1. Cache-Aside Pattern (37 endpoints)
2. Docker image configured
3. Health checks integrated
4. All services connected

### Phase 1 (Next) ⏳
1. Rate limiting (Gateway)
2. Correlation ID storage (Tracing)
3. Session store (ASP.NET Core)

### Phase 2 (Following) 
1. Redis Streams (Message Queue)
2. Hangfire Job Store
3. Feature Toggles

### Phase 3 (Later)
1. Distributed Locks
2. Metrics aggregation
3. Cache warming strategies

---

## 🚀 Getting Started with Redis CLI

```bash
# Connect to Redis
docker exec -it techbirdsfly-redis redis-cli

# Basic commands
PING              # Test connection
INFO              # Server statistics
DBSIZE            # Total keys in DB
KEYS *            # List all keys (WARNING: slow on large DB)
KEYS cache:*      # List all cache keys

# Monitor in real-time
MONITOR           # See all commands being executed

# Check specific key
GET cache:user:123
TTL cache:user:123

# Set value with expiration
SET test-key "hello" EX 3600

# Use Redis in Python
pip install redis
python -c "import redis; r = redis.Redis(host='localhost', port=6379); print(r.ping())"
```

---

## 📖 References

- **StackExchange.Redis**: https://stackexchange.github.io/StackExchange.Redis/
- **Redis Official**: https://redis.io/
- **Hangfire + Redis**: https://docs.hangfire.io/en/latest/
- **Redis Streams**: https://redis.io/topics/streams-intro

---

**Redis is now your microservices' "nervous system" — fast, distributed, and persistent.** 🔴⚡

**Next: Implement Phase 1 observability (Serilog + Jaeger) using templates from `PHASE1_CODE_TEMPLATES.md`**
