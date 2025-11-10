using ImageService.Infrastructure;
using ImageService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using Serilog;
using System.Text;

// ============================================================================
// SERILOG BOOTSTRAP - MUST BE FIRST
// ============================================================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Service", "ImageService")
    .CreateBootstrapLogger();

try
{
    Log.Information("🖼️  Image Service - Initializing Application");

    var builder = WebApplication.CreateBuilder(args);

    // ========================================================================
    // SERILOG CONFIGURATION
    // ========================================================================

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var serviceName = "ImageService";

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

    // Simplified for now - will add full instrumentation when packages are configured
    builder.Services.AddOpenTelemetry();

    // ========================================================================
    // CORE SERVICES
    // ========================================================================

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ========================================================================
    // AUTHENTICATION & AUTHORIZATION
    // ========================================================================

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

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("UserOrAdmin", policy =>
            policy.RequireRole("User", "Admin"));
    });

    // ========================================================================
    // INFRASTRUCTURE SERVICES - CLEAN ARCHITECTURE
    // ========================================================================

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration.GetConnectionString("ImageDb")
        ?? "Data Source=image.db";

    builder.Services.AddInfrastructureServices(connectionString);

    // ========================================================================
    // SWAGGER/OPENAPI CONFIGURATION
    // ========================================================================

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Image Service API",
            Version = "v1.0",
            Description = "AI-powered image generation and management service (Clean Architecture)",
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

    // ========================================================================
    // CORS CONFIGURATION
    // ========================================================================

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", corsBuilder =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:5004" };

            corsBuilder
                .WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // ========================================================================
    // HEALTH CHECKS
    // ========================================================================

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ImageDbContext>(name: "database");

    // ========================================================================
    // BUILD APPLICATION
    // ========================================================================

    var app = builder.Build();

    // ========================================================================
    // INITIALIZE DATABASE
    // ========================================================================

    Log.Information("Initializing database and applying migrations...");
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ImageDbContext>();
            await context.Database.MigrateAsync();
            Log.Information("✅ Database initialization completed successfully");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Database initialization failed");
        throw;
    }

    // ========================================================================
    // MIDDLEWARE PIPELINE
    // ========================================================================

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Image Service API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks("/health");
    app.MapControllers();

    // ========================================================================
    // STARTUP BANNER
    // ========================================================================

    Log.Information("═══════════════════════════════════════════════════════════════════════════════════");
    Log.Information("✅ 🖼️  IMAGE SERVICE - CLEAN ARCHITECTURE - INITIALIZED SUCCESSFULLY");
    Log.Information("═══════════════════════════════════════════════════════════════════════════════════");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Port: {Port}", 5004);
    Log.Information("Swagger UI: http://localhost:5004/swagger");
    Log.Information("Health Check: http://localhost:5004/health");
    Log.Information("═══════════════════════════════════════════════════════════════════════════════════");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush();
}
