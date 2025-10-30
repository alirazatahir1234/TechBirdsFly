using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TechBirdsFly.Shared.Configuration;

/// <summary>
/// Reusable Swagger/OpenAPI configuration for all TechBirdsFly microservices
/// Provides consistent branding, authentication, and documentation
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Add Swagger Gen with TechBirdsFly branding and JWT Bearer security
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceName">Name of the microservice (e.g., "Auth Service")</param>
    /// <param name="serviceVersion">API version (e.g., "v1")</param>
    /// <param name="description">Service description</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddTechBirdsFlSwagger(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion = "v1",
        string description = "")
    {
        services.AddSwaggerGen(c =>
        {
            // Basic service information
            c.SwaggerDoc(serviceVersion, new OpenApiInfo
            {
                Title = $"TechBirdsFly {serviceName}",
                Version = serviceVersion,
                Description = string.IsNullOrWhiteSpace(description)
                    ? $"Microservice for {serviceName.ToLower()} in the TechBirdsFly platform"
                    : description,
                Contact = new OpenApiContact
                {
                    Name = "TechBirdsFly Team",
                    Email = "support@techbirdsfly.com",
                    Url = new Uri("https://techbirdsfly.com")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://techbirdsfly.com/terms")
            });

            // JWT Bearer authentication scheme
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = @"JWT Authorization header using the Bearer scheme.
Example: ""Authorization: Bearer {token}""",
                In = ParameterLocation.Header
            });

            // Add Bearer token requirement to all endpoints
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    Array.Empty<string>()
                }
            });

            // Add X-Correlation-ID header documentation
            c.AddSecurityDefinition("X-Correlation-ID", new OpenApiSecurityScheme
            {
                Name = "X-Correlation-ID",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Correlation ID for request tracing"
            });

            // Enable XML comments if available
            try
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (System.IO.File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            }
            catch
            {
                // XML comments are optional
            }

            // Set default model expansion depth
            c.SchemaFilter<EnumSchemaFilter>();
        });

        return services;
    }

    /// <summary>
    /// Configure Swagger UI with TechBirdsFly branding
    /// </summary>
    /// <param name="app">Web application builder</param>
    /// <param name="serviceName">Name of the microservice</param>
    /// <param name="apiVersion">API version (e.g., "v1")</param>
    /// <param name="routePrefix">Route prefix for Swagger UI (default: root)</param>
    /// <returns>Web application for chaining</returns>
    public static WebApplication UseTechBirdsFlSwagger(
        this WebApplication app,
        string serviceName,
        string apiVersion = "v1",
        string routePrefix = "")
    {
        if (!app.Environment.IsEnvironment("Development"))
        {
            return app; // Only serve Swagger in Development
        }

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        // Must be outside IsDevelopment block so static files load
        app.UseStaticFiles();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(
                $"/swagger/{apiVersion}/swagger.json",
                $"TechBirdsFly {serviceName} {apiVersion}");

            c.RoutePrefix = routePrefix;

            // UI Configuration
            c.DefaultModelsExpandDepth(2);
            c.DefaultModelExpandDepth(2);
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.DisplayOperationId();
            c.ShowExtensions();

            // Add custom CSS for TechBirdsFly branding
            c.InjectStylesheet("/swagger-custom.css");
        });

        return app;
    }

    /// <summary>
    /// Enable static files middleware (must be called before UseSwaggerUI)
    /// </summary>
    public static WebApplication UseSwaggerStaticFiles(this WebApplication app)
    {
        app.UseStaticFiles();
        return app;
    }
}

/// <summary>
/// Custom schema filter for better enum documentation
/// </summary>
internal class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Description += $" ({string.Join(", ", Enum.GetNames(context.Type))})";
        }
    }
}
