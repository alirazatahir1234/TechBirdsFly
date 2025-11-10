using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BillingService.Infrastructure;
using BillingService.Infrastructure.Persistence;
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
    Log.Information("Starting TechBirdsFly Billing Service");

    var builder = WebApplication.CreateBuilder(args);

    // ========================================================================
    // CONFIGURE SERILOG
    // ========================================================================

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var serviceName = "BillingService";

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

    var serviceName2 = "BillingService";
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

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();

    // EF Core SQLite
    builder.Services.AddDbContext<BillingDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("BillingDb") ?? "Data Source=billing.db"));

    // Add Billing Services (DI)
    builder.Services.AddBillingServices(builder.Configuration);

    // ========================================================================
    // BUILD APP & MIDDLEWARE PIPELINE
    // ========================================================================

    var app = builder.Build();

    // Migrate DB
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        db.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Request/Response logging with correlation ID
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health checks endpoint
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TechBirdsFly Billing Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
