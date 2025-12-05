using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TemplateService.Application.Handlers;

namespace TemplateService.Application.Extensions;

/// <summary>
/// Dependency injection extension for Application layer
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateTemplateHandler).Assembly);
        });

        return services;
    }
}
