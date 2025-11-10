using Serilog;
using Microsoft.EntityFrameworkCore;
using TechBirdsFly.AdminService.Infrastructure.Persistence;
using TechBirdsFly.AdminService.WebAPI.DI;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Configure Serilog logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Admin Services (DbContext, Repositories, Services, EventPublisher)
builder.Services.AddAdminServices(builder.Configuration);

// TODO: Add OpenTelemetry for distributed tracing
// builder.Services.AddOpenTelemetryInstrumentation(builder.Configuration, "AdminService");

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AdminDbContext>(name: "database", tags: new[] { "ready" })
    .AddUrlGroup(
        new Uri(builder.Configuration["EventBusService:Url"] ?? "http://localhost:5020"),
        name: "event-bus",
        tags: new[] { "ready" });

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Admin Service API",
        Version = "v1",
        Description = "Admin Service for managing system administrators, roles, and audit logs",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "TechBirdsFly",
            Email = "support@techbirdsfly.com"
        }
    });

    // Include XML documentation if available
    var xmlFile = "AdminService.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Map controllers
app.MapControllers();

// Run database migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        Log.Information("Running database migrations...");
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations completed successfully");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while running database migrations");
    throw;
}

try
{
    Log.Information("Starting Admin Service");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Admin Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
