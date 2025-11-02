using Serilog;
using Serilog.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EventBusService.WebAPI.DI;
using TechBirdsFly.Shared.Configuration;

// Configure Serilog
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

try
{
    logger.Information("🚀 Starting TechBirdsFly Event Bus Service");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog(logger);

    // Add services to the container
    builder.Services.AddEventBusServices(builder.Configuration);

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = jwtSettings.GetValue<string>("SecretKey")
        ?? throw new InvalidOperationException("JWT SecretKey not configured");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                ValidateAudience = true,
                ValidAudience = jwtSettings.GetValue<string>("Audience"),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        });

    builder.Services.AddAuthorization();

    // Swagger
    builder.Services.AddTechBirdsFlSwagger(
        serviceName: "Event Bus Service",
        serviceVersion: "v1",
        description: "Distributed event bus for microservices communication with Kafka & Outbox pattern");

    // Controllers
    builder.Services.AddControllers();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policyBuilder =>
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseTechBirdsFlSwagger("Event Bus Service", apiVersion: "v1", routePrefix: "");
        app.UseSwaggerStaticFiles();
    }

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors("AllowAll");

    app.MapControllers();
    app.MapHealthChecks("/health");

    logger.Information("✅ Event Bus Service initialized successfully. Listening on ports 5030/7030");
    await app.RunAsync();
}
catch (Exception ex)
{
    logger.Fatal(ex, "❌ Event Bus Service terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
