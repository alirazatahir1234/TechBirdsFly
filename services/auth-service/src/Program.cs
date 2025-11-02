using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Context;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

    // Redis check is optional - only add if running with Docker/dedicated Redis instance
    if (builder.Configuration.GetValue<bool>("Features:IncludeRedisHealthCheck"))
    {
        builder.Services.AddHealthChecks()
            .AddRedis(
                builder.Configuration["ConnectionStrings:Redis"] ?? "localhost:6379",
                name: "Redis");
    }

    // =========================================================================
    // DEPENDENCY INJECTION LAYERS
    // =========================================================================
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // =========================================================================
    // JWT AUTHENTICATION
    // =========================================================================
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "development-secret-key-please-change";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TechBirdsFly";
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

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        db.Database.Migrate();
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

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ TechBirdsFly Auth Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
