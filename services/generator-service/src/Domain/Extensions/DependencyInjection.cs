using Microsoft.OpenApi.Models;
using GeneratorService.WebAPI.Middleware;
using MediatR;

namespace GeneratorService.WebAPI.Extensions;

/// <summary>
/// Dependency injection extensions for WebAPI layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds WebAPI services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services)
    {
        // MediatR - Command/Query mediator pattern
        // Register handlers from Application assembly
        services.AddMediatR(typeof(GeneratorService.Application.Features.GenerateWebsite.GenerateWebsiteCommand));

        // Controllers
        services.AddControllers();

        // API Documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TechBirdsFly Generator Service API",
                Version = "v1.0",
                Description = "AI-powered website generation microservice",
                Contact = new OpenApiContact
                {
                    Name = "TechBirdsFly Development",
                    Email = "dev@techbirdsfly.com"
                }
            });
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        // Health Checks
        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// Configures the WebAPI middleware pipeline
    /// </summary>
    public static IApplicationBuilder UseWebAPIPipeline(this IApplicationBuilder app, IWebHostEnvironment? env = null)
    {
        // Error handling
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // CORS
        app.UseCors("AllowFrontend");

        // Swagger UI (always enabled for API inspection)
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Generator Service API v1");
            options.RoutePrefix = "swagger";
        });

        // Routing
        app.UseRouting();

        return app;
    }
}
