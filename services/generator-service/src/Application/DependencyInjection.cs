using Microsoft.Extensions.DependencyInjection;
using MediatR;
using GeneratorService.Application.Behaviors;
using GeneratorService.Application.Mapping;

namespace GeneratorService.Application;

/// <summary>
/// Dependency Injection for Application layer services
/// Registers MediatR, AutoMapper, validators, and request behaviors
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper with assembly scan for profiles
        var assemblyToScan = typeof(DependencyInjection).Assembly;
        services.AddAutoMapper(assemblyToScan);

        // Register MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Register request/response logging behavior
            config.AddRequestPreProcessor(typeof(LoggingBehavior<>));
        });

        // Register validators
        services.AddValidatorsAsTransient();

        // Register behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        return services;
    }

    private static void AddValidatorsAsTransient(this IServiceCollection services)
    {
        // Validators will be auto-discovered by MediatR
        var assembly = typeof(DependencyInjection).Assembly;
        var validatorType = typeof(FluentValidation.IValidator<>);

        var validators = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == validatorType))
            .ToList();

        foreach (var validator in validators)
        {
            var validatorInterface = validator.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorType);

            services.AddTransient(validatorInterface, validator);
        }
    }
}
