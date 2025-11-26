using Microsoft.EntityFrameworkCore;
using GeneratorService.Infrastructure;
using GeneratorService.Infrastructure.Persistence;
using GeneratorService.Middleware;
using GeneratorService.WebAPI.Extensions;
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
    Log.Information("Starting TechBirdsFly Generator Service");

    var builder = WebApplication.CreateBuilder(args);

    // ========================================================================
    // CONFIGURE SERILOG
    // ========================================================================

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var serviceName = "GeneratorService";

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

    var serviceName2 = "GeneratorService";
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

    // Infrastructure services (Database, Repositories, AI services)
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // WebAPI services (Controllers, Swagger, CORS, health checks)
    builder.Services.AddWebAPIServices();

    // ========================================================================
    // BUILD APP & MIDDLEWARE PIPELINE
    // ========================================================================

    var app = builder.Build();

    // Initialize database
    await app.Services.InitializeDatabaseAsync();

    // Configure WebAPI pipeline (error handling, CORS, Swagger)
    app.UseWebAPIPipeline();

    // Request/Response logging with correlation ID
    app.UseSerilogRequestLogging();

    // Add correlation ID to all requests
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.MapControllers();

    // Health checks endpoint
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TechBirdsFly Generator Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
