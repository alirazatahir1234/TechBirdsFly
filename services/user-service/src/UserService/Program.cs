using UserService.Data;
using UserService.Middleware;
using UserService.Services;
using UserService.Services.Cache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;
using System.Text;

// Bootstrap Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq("http://seq:80")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("ServiceName", "UserService")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to DI
    builder.Host.UseSerilog();

    // ============================================================================
    // SERVICES
    // ============================================================================

    // Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=user.db";
    builder.Services.AddDbContext<UserDbContext>(options =>
    {
        options.UseSqlite(connectionString);
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine);
        }
    });

    // Add Redis Distributed Cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        options.Configuration = redisConnectionString;
        options.InstanceName = "UserService_";
    });

    // Register Cache Service
    builder.Services.AddScoped<ICacheService, RedisCacheService>();

    // Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings.GetValue<string>("SecretKey")
        ?? throw new InvalidOperationException("JWT SecretKey not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
            ValidateAudience = true,
            ValidAudience = jwtSettings.GetValue<string>("Audience"),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

    // Authorization
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("UserOrAdmin", policy =>
            policy.RequireRole("User", "Admin"));
    });

    // Application Services
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

    // Controllers
    builder.Services.AddControllers();

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "User Service API",
            Version = "v1.0",
            Description = "User management and subscription service",
            Contact = new OpenApiContact
            {
                Name = "TechBirdsFly Team",
                Email = "support@techbirdsfly.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", corsBuilder =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:3001" };

            corsBuilder
                .WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: "UserService", serviceVersion: "1.0.0"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST") ?? "localhost";
                    options.AgentPort = int.Parse(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT") ?? "6831");
                });
        });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<UserDbContext>(name: "database");

    // ============================================================================
    // MIDDLEWARE PIPELINE
    // ============================================================================

    var app = builder.Build();

    // Database Migration
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        db.Database.Migrate();
        Log.Information("✓ Database migrations applied");
    }

    // Development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // Observability Middleware
    app.UseSerilogRequestLogging();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // HTTPS Redirect
    app.UseHttpsRedirection();

    // CORS
    app.UseCors("AllowFrontend");

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Health Check Endpoint
    app.MapHealthChecks("/health");

    // Controllers
    app.MapControllers();

    // Startup Banner
    Log.Information("═══════════════════════════════════════════════════════════════");
    Log.Information("👥 User Service Starting");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Database: SQLite");
    Log.Information("═══════════════════════════════════════════════════════════════");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
