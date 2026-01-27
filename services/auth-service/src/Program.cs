using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Context;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TechBirdsFly.CacheClient;

using AuthService.Infrastructure.Persistence;
using AuthService.WebAPI.Middlewares;
using AuthService.WebAPI.DI;
using TechBirdsFly.Shared.Configuration;

// ============================================================================
// SERILOG BOOTSTRAP LOGGER  (MUST BE FIRST)
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting TechBirdsFly Auth Service");

    var builder = WebApplication.CreateBuilder(args);

    // =========================================================================
    // CLOUD RUN PORT CONFIGURATION
    // =========================================================================
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    // =========================================================================
    // SERILOG CONFIGURATION
    // =========================================================================
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Service", "AuthService")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        // Optional: write to Seq if configured
        var seqUrl = context.Configuration["Serilog:Seq:Url"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            configuration.WriteTo.Seq(
                serverUrl: seqUrl,
                apiKey: context.Configuration["Serilog:Seq:ApiKey"]);
        }
    });

    // =========================================================================
    // OPENTELEMETRY TRACING
    // =========================================================================
    var resource = ResourceBuilder.CreateDefault()
        .AddService(serviceName: "AuthService", serviceVersion: "1.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            { "environment", builder.Environment.EnvironmentName },
            { "service.namespace", "techbirdsfly" }
        });

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .SetResourceBuilder(resource)
            .AddAspNetCoreInstrumentation(o =>
            {
                o.RecordException = true;
                o.Filter = ctx => !ctx.Request.Path.ToString().Contains("/health");
            })
            .AddHttpClientInstrumentation(o => o.RecordException = true)
            .AddJaegerExporter(o =>
            {
                o.AgentHost = builder.Configuration["Jaeger:AgentHost"] ?? "localhost";
                o.AgentPort = int.Parse(builder.Configuration["Jaeger:AgentPort"] ?? "6831");
            }));

    // =========================================================================
    // CORE SERVICES
    // =========================================================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ✅ Use TechBirdsFly Swagger Configuration Template
    builder.Services.AddTechBirdsFlSwagger(
        serviceName: "Auth Service",
        serviceVersion: "v1",
        description: "Authentication & JWT token management for TechBirdsFly platform");


    // =========================================================================
    // HEALTH CHECKS
    // =========================================================================
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AuthDbContext>("Database");

    // =========================================================================
    // DEPENDENCY INJECTION LAYERS
    // =========================================================================
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Get JWT configuration (used for both cache client and authentication)
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "development-secret-key-please-change";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TechBirdsFly";

    // Centralized Cache Client (replaces local Redis)
    // Skip in test environments
    if (!builder.Environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var cacheServiceUrl = builder.Configuration["Services:CacheService:Url"] ?? "http://localhost:8100";
            builder.Services.AddCacheClient(cacheServiceUrl, jwtKey);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "⚠️ Cache client initialization failed (expected in test environments)");
        }
    }

    var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

    // =========================================================================
    // BUILD APP & DATABASE MIGRATION
    // =========================================================================
    var app = builder.Build();

    // Only run migrations in production/development, not in test environments
    if (!app.Environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                db.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "⚠️ Database migration failed (this is expected in test environments)");
        }
    }

    // =========================================================================
    // MIDDLEWARE PIPELINE
    // =========================================================================
    if (app.Environment.IsDevelopment())
    {
        // ✅ Use TechBirdsFly Swagger UI Configuration
        app.UseTechBirdsFlSwagger(
            serviceName: "Auth Service",
            apiVersion: "v1",
            routePrefix: "");
    }

    // ✅ Static files MUST be outside the IsDevelopment() block
    app.UseSwaggerStaticFiles();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // HEALTH ENDPOINT
    app.MapHealthChecks("/health");

    // Prevent Kestrel from starting during integration tests
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_TEST_HOST") == "true")
    {
        return;
    }

    // Only run when NOT in test environment
    if (!app.Environment.IsEnvironment("Test"))
    {
        app.Run();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ TechBirdsFly Auth Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }


