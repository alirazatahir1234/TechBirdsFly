using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TechBirdsFly";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TechBirdsFly";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT Token validated for user: {User}", 
                    context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset");
    });
});

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter: 100 requests per minute per user
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var username = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: username,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // Per-User Rate Limit Policy (authenticated users)
    options.AddPolicy("PerUserRateLimit", context =>
    {
        var username = context.User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10, // Lower limit for anonymous
                    Window = TimeSpan.FromMinutes(1)
                });
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            username,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    // IP-based Rate Limit Policy (DDoS protection)
    options.AddPolicy("PerIpRateLimit", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetSlidingWindowLimiter(
            ipAddress,
            _ => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 50,
                Window = TimeSpan.FromSeconds(30),
                SegmentsPerWindow = 3
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.Append("Retry-After", retryAfter.TotalSeconds.ToString());
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "Rate limit exceeded. Please try again later.",
            statusCode = 429
        }, cancellationToken: token);

        Log.Warning("Rate limit exceeded for {User} from {IP}", 
            context.HttpContext.User.Identity?.Name ?? "anonymous",
            context.HttpContext.Connection.RemoteIpAddress);
    };
});

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("http://localhost:5001/health"), name: "auth-service", timeout: TimeSpan.FromSeconds(3))
    .AddUrlGroup(new Uri("http://localhost:5002/health"), name: "user-service", timeout: TimeSpan.FromSeconds(3))
    .AddUrlGroup(new Uri("http://localhost:5003/health"), name: "generator-service", timeout: TimeSpan.FromSeconds(3))
    .AddUrlGroup(new Uri("http://localhost:5007/health"), name: "image-service", timeout: TimeSpan.FromSeconds(3))
    .AddUrlGroup(new Uri("http://localhost:5008/health"), name: "user-profile-service", timeout: TimeSpan.FromSeconds(3));

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TechBirdsFly API Gateway",
        Version = "v1",
        Description = "YARP-based API Gateway with JWT validation, rate limiting, and CORS"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechBirdsFly Gateway v1");
        c.RoutePrefix = "swagger";
        c.DefaultModelsExpandDepth(-1);  // Hide schemas by default
    });
}

// Serve static files for Swagger UI
app.UseStaticFiles();

// Request logging
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    
    Log.Information("Incoming Request: {Method} {Path} from {IP}", 
        context.Request.Method, 
        context.Request.Path, 
        context.Connection.RemoteIpAddress);
    
    await next();
    
    var duration = DateTime.UtcNow - startTime;
    Log.Information("Completed Request: {Method} {Path} {StatusCode} in {Duration}ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        duration.TotalMilliseconds);
});

// CORS must be before Authentication/Authorization
app.UseCors("AllowFrontend");

// Rate Limiting
app.UseRateLimiter();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Check Endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            services = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Gateway Info Endpoint
app.MapGet("/info", () => new
{
    name = "TechBirdsFly API Gateway",
    version = "1.0.0",
    timestamp = DateTime.UtcNow,
    features = new[]
    {
        "JWT Authentication",
        "Rate Limiting (100 req/min)",
        "CORS Protection",
        "Service Health Monitoring",
        "Request Logging",
        "Load Balancing"
    },
    routes = new
    {
        auth = "/api/auth/** → Auth Service (5001)",
        users = "/api/users/** → User Service (5008)",
        projects = "/api/projects/** → Generator Service (5003)",
        images = "/api/images/** → Image Service (5007)",
        admin = "/api/admin/** → Admin Service (5006)"
    }
}).WithName("GatewayInfo");

// Map YARP Reverse Proxy
app.MapReverseProxy();

Log.Information("🚀 TechBirdsFly API Gateway starting on port 5000");
Log.Information("✅ JWT Authentication: Enabled");
Log.Information("✅ Rate Limiting: 100 requests/min per user, 50 requests/30s per IP");
Log.Information("✅ CORS: Configured for frontend origins");
Log.Information("✅ Health Checks: Monitoring 5 downstream services");

app.Run();
