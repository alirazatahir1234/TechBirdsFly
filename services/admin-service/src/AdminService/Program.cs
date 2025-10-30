using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AdminService.Data;
using AdminService.Models;
using AdminService.Services;
using AdminService.Services.Cache;
using AdminService.Hubs;
using AdminService.Middleware;
using Serilog;
using Serilog.Context;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// ============================================================================
// SERILOG CONFIGURATION - MUST BE FIRST
// ============================================================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TechBirdsFly Admin Service");
    
    var builder = WebApplication.CreateBuilder(args);

    // ========================================================================
    // CONFIGURE SERILOG
    // ========================================================================
    
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var serviceName = "AdminService";
        
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(
                serverUrl: context.Configuration["Serilog:Seq:Url"] ?? "http://seq:80",
                apiKey: context.Configuration["Serilog:Seq:ApiKey"])
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
    });

    // ========================================================================
    // OPENTELEMETRY CONFIGURATION
    // ========================================================================

    var serviceName2 = "AdminService";
    var serviceVersion = "1.0.0";

    var resource = ResourceBuilder.CreateDefault()
        .AddService(serviceName2, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            { "environment", builder.Environment.EnvironmentName },
            { "service.namespace", "techbirdsfly" }
        });

    var otelBuilder = builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .SetResourceBuilder(resource)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = ctx => !ctx.Request.Path.ToString().Contains("/health");
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["Jaeger:AgentHost"] ?? "localhost";
                options.AgentPort = int.Parse(builder.Configuration["Jaeger:AgentPort"] ?? "6831");
            }));

    // ========================================================================
    // ADD SERVICES
    // ========================================================================

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // SignalR for real-time communication
    builder.Services.AddSignalR();

    // EF Core SQLite
    builder.Services.AddDbContext<AdminDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("AdminDb") ?? "Data Source=admin.db");
        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    });

    // Add Redis Distributed Cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        options.Configuration = redisConnectionString;
        options.InstanceName = "AdminService_";
    });

    // Register Cache Service
    builder.Services.AddScoped<ICacheService, RedisCacheService>();

    // JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-change-in-production";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TechBirdsFly";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = false,
                ValidateLifetime = true
            };
        });

    // Services
    builder.Services.AddScoped<IAdminService, global::AdminService.Services.AdminService>();
    builder.Services.AddScoped<IUserManagementService, global::AdminService.Services.UserManagementService>();
    builder.Services.AddScoped<IRoleManagementService, global::AdminService.Services.RoleManagementService>();
    builder.Services.AddScoped<IAnalyticsService, global::AdminService.Services.AnalyticsService>();
    builder.Services.AddScoped<IRealTimeService, global::AdminService.Services.RealTimeService>();

    // ========================================================================
    // BUILD APP & MIDDLEWARE PIPELINE
    // ========================================================================

    var app = builder.Build();

    // Migrate DB
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        db.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Request/Response logging with correlation ID
    app.UseSerilogRequestLogging();

    // Add correlation ID to all requests
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Global exception handling
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health checks endpoint
    app.MapHealthChecks("/health");

    // Map SignalR hub
    app.MapHub<AdminHub>("/hubs/admin");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TechBirdsFly Admin Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
