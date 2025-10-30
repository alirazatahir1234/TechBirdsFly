# Redis Cache Implementation Guide

## Overview

This guide covers the **shared Redis caching infrastructure** integrated across all TechBirdsFly microservices (.NET 8).

Redis acts as a **centralized, high-performance cache** for:
- Session tokens (Auth Service)
- Billing summaries & usage metrics (Billing Service)
- User data & settings (User Service)
- Admin dashboards & analytics (Admin Service)
- Image generation results (Image Service)

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Redis 7.4-Alpine                          │
│                (Shared Cache Container)                      │
│                     :6379                                    │
└─────────────────────────────────────────────────────────────┘
         ↑                                    ↑
    ┌────┴────┐  ┌────────────┐  ┌──────────┴────┐
    │   Auth  │  │  Billing   │  │ Generator     │
    │ Service │  │  Service   │  │ Service       │
    └────┬────┘  └────────────┘  └──────────┬────┘
         │                             │
    ┌────┴────┐  ┌────────────┐  ┌──────────┴────┐
    │  Admin  │  │   Image    │  │ User          │
    │ Service │  │  Service   │  │ Service       │
    └─────────┘  └────────────┘  └───────────────┘
```

---

## Quick Start

### 1. Start Redis Standalone

```bash
cd infra/redis
docker-compose up -d
```

✓ Redis runs on `localhost:6379`

### 2. Start All Services with Redis

```bash
cd infra
docker-compose up -d redis auth-service billing-service generator-service admin-service image-service user-service
```

Or start everything at once:

```bash
docker-compose up -d
```

### 3. Verify Redis Connection

```bash
docker exec -it techbirdsfly-redis redis-cli ping
# Response: PONG
```

---

## Configuration

### Each Service

Every microservice includes:

```json
{
  "ConnectionStrings": {
    "Redis": "redis:6379"  // or localhost:6379 when running locally
  }
}
```

### Program.cs Registration

```csharp
// Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ServiceName_";  // Prevents key collisions
});

// Register Cache Service
builder.Services.AddScoped<ICacheService, RedisCacheService>();
```

### Key Naming Convention

To prevent collisions between services, prefix all keys with the service name:

```
AuthService_auth:token:{userId}
AuthService_user:{userId}
BillingService_billing:summary:{accountId}
BillingService_usage:{userId}
ImageService_image:result:{imageId}
```

---

## Usage Examples

### Auth Service

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest req)
{
    var tokens = await _auth.LoginAsync(req.Email, req.Password);
    
    // Cache token for 1 hour
    await _cache.SetAsync(
        $"token:{req.Email}", 
        tokens.accessToken, 
        TimeSpan.FromHours(1)
    );
    
    return Ok(new { accessToken = tokens.accessToken, refreshToken = tokens.refreshToken });
}

[HttpPost("logout")]
public async Task<IActionResult> Logout([FromQuery] string email)
{
    // Remove token from cache
    await _cache.RemoveAsync($"token:{email}");
    return Ok(new { message = "Logged out" });
}
```

### Billing Service

```csharp
[HttpGet("summary/{accountId}")]
public async Task<IActionResult> GetBillingSummary(int accountId)
{
    string key = $"billing:summary:{accountId}";
    
    // Try cache first
    var cached = await _cache.GetAsync<BillingSummary>(key);
    if (cached != null)
        return Ok(new { fromCache = true, data = cached });

    // Fetch from DB
    var summary = await _billingService.GetSummaryAsync(accountId);
    
    // Cache for 15 minutes
    await _cache.SetAsync(key, summary, TimeSpan.FromMinutes(15));
    
    return Ok(new { fromCache = false, data = summary });
}
```

### Admin Service (Real-Time Analytics)

```csharp
[HttpGet("analytics/dashboard")]
public async Task<IActionResult> GetDashboard()
{
    var key = "analytics:dashboard";
    
    var cached = await _cache.GetAsync<DashboardData>(key);
    if (cached != null)
        return Ok(cached);
    
    // Compute analytics
    var data = await _analyticsService.ComputeDashboardAsync();
    
    // Cache for 5 minutes
    await _cache.SetAsync(key, data, TimeSpan.FromMinutes(5));
    
    return Ok(data);
}
```

---

## Cache Expiry Strategy

| Service       | Data Type              | TTL      | Reason                    |
|---------------|------------------------|----------|---------------------------|
| Auth          | Tokens                 | 1 hour   | Session duration          |
| Auth          | User profiles          | 5 min    | User data changes often   |
| Billing       | Account summaries      | 15 min   | Changes with usage        |
| Billing       | Current usage          | 5 min    | Real-time tracking        |
| Billing       | Invoices               | 30 min   | Rarely changes            |
| Admin         | Dashboard analytics    | 5 min    | Near-real-time            |
| Image         | Generation metadata    | 1 hour   | Stable data               |
| User          | Preferences            | 10 min   | User updates              |

---

## Monitoring & Management

### View All Keys

```bash
docker exec -it techbirdsfly-redis redis-cli
> KEYS *
> KEYS AuthService_*
> KEYS BillingService_*
```

### View Specific Key

```bash
> GET billing:summary:123
```

### Monitor Commands Real-Time

```bash
> MONITOR
```

### Check Memory Usage

```bash
> INFO MEMORY
used_memory: 1.23M
maxmemory: 256M
```

### Clear Cache (Development Only!)

```bash
> FLUSHALL
```

### RedisInsight GUI (Optional)

For a visual interface:

```bash
# Download from https://redis.io/insight/
# Connect to localhost:6379 in the GUI
```

---

## Performance Benefits

### Before Redis (DB Lookups Every Time)
```
User Login → Query Auth DB (50ms) → Query User DB (50ms) → Total: 100ms
```

### After Redis (Cached)
```
User Login → Redis Lookup (< 1ms) → Total: < 1ms  ✅ 100x faster!
```

### Benchmark Results (Expected)

| Operation    | Without Cache | With Cache | Improvement |
|--------------|---------------|------------|-------------|
| Get user profile | 50ms | <1ms | 50x faster |
| Get billing summary | 100ms | <1ms | 100x faster |
| Dashboard load | 500ms | 5ms | 100x faster |

---

## Docker Networking

All services communicate via `techbirdsfly_network`:

```yaml
services:
  auth-service:
    networks:
      - techbirdsfly_network
    depends_on:
      redis:
        condition: service_healthy
```

**From within container:** `redis://techbirdsfly-redis:6379`  
**From host machine:** `redis://localhost:6379`

---

## Production Deployment

### Azure Cache for Redis

Replace local Redis with managed service:

```json
"ConnectionStrings": {
  "Redis": "myredis.redis.cache.windows.net:6380,password=XXXXX,ssl=True,abortConnect=False"
}
```

### High Availability Setup

```yaml
redis-master:
  image: redis:7.4-alpine
  
redis-slave:
  image: redis:7.4-alpine
  command: redis-server --slaveof redis-master 6379
  
redis-sentinel:
  image: redis:7.4-alpine
  command: redis-sentinel /etc/redis/sentinel.conf
```

### Replication & Backup

```bash
# Enable AOF (Append-Only File)
appendonly yes
appendfsync everysec

# RDB Snapshots
save 900 1      # Every 15 min if 1+ keys changed
save 300 10     # Every 5 min if 10+ keys changed
```

---

## Troubleshooting

### Redis Connection Refused

```bash
# Check if container is running
docker ps | grep redis

# Check logs
docker logs techbirdsfly-redis

# Restart
docker restart techbirdsfly-redis
```

### High Memory Usage

```bash
# Check memory
docker exec -it techbirdsfly-redis redis-cli INFO MEMORY

# Increase maxmemory in redis.conf
maxmemory 512mb

# Change eviction policy
maxmemory-policy allkeys-lru  # Remove least recently used
```

### Keys Not Persisting

```bash
# Verify persistence is enabled
docker exec -it techbirdsfly-redis redis-cli CONFIG GET appendonly
# Result: "yes"

# Check AOF file size
docker exec -it techbirdsfly-redis ls -lh /data
```

### Service Can't Connect to Redis

```bash
# Test from service container
docker exec techbirdsfly-auth-service curl http://techbirdsfly-redis:6379

# Check network
docker network inspect techbirdsfly_network

# Verify service environment
docker exec techbirdsfly-auth-service env | grep REDIS
```

---

## Best Practices

✅ **DO**
- Use meaningful key prefixes (ServiceName_)
- Set appropriate TTLs per use case
- Monitor cache hit ratios
- Invalidate cache when data changes
- Use health checks for Redis
- Handle cache misses gracefully

❌ **DON'T**
- Store passwords or secrets in cache
- Cache large blobs (> 1MB)
- Set infinite TTLs
- Rely on cache alone (always have DB)
- Ignore cache connection failures
- Use default port in production (use non-standard port + auth)

---

## Integration with Services

### Where Cache is Currently Used

1. **Auth Service**
   - Token validation (validate-token endpoint)
   - User session data
   - Login/logout operations

2. **Billing Service**
   - Account summaries (15-min TTL)
   - Current usage tracking (5-min TTL)
   - Invoice lists (30-min TTL)

3. **Admin Service** (Ready)
   - Dashboard analytics (5-min TTL)
   - System metrics

4. **Image/User Services** (Ready)
   - User preferences
   - Image metadata

---

## Next Steps

1. **Start Redis:** `cd infra/redis && docker-compose up -d`
2. **Test Connectivity:** `docker exec -it techbirdsfly-redis redis-cli ping`
3. **Deploy Services:** `docker-compose up -d`
4. **Monitor:** Use `redis-cli MONITOR` during testing
5. **Optimize:** Adjust TTLs based on usage patterns

---

## References

- [Redis Official Docs](https://redis.io/documentation)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Docker Redis Image](https://hub.docker.com/_/redis)
- [Microsoft.Extensions.Caching.Redis](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.stackexchangeredis)

