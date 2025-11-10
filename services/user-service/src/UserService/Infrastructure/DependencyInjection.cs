using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Infrastructure.ExternalServices;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString,
        string jwtSecret)
    {
        // Database
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        // External Services
        services.AddSingleton<ITokenService>(sp =>
            new TokenService(jwtSecret, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TokenService>>()));
        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<ICacheService, CacheService>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserApplicationService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            try
            {
                // Apply migrations
                await context.Database.MigrateAsync();

                // Seed initial data if needed
                if (!await context.Users.AnyAsync())
                {
                    // Seed admin user for testing
                    var passwordHasher = new PasswordHashService(scope.ServiceProvider.GetRequiredService<ILogger<PasswordHashService>>());
                    var passwordHash = passwordHasher.HashPassword("Admin@123456");

                    var adminUserResult = User.Create(
                        "admin",
                        new EmailAddress("admin@techbirdsfly.com"),
                        passwordHash,
                        "Admin User",
                        new PhoneNumber("+1234567890"));

                    if (adminUserResult.IsSuccess && adminUserResult.Data != null)
                    {
                        var adminUser = adminUserResult.Data;
                        adminUser.AssignRole(UserRole.Admin);
                        adminUser.VerifyEmail();

                        context.Users.Add(adminUser);
                        await context.SaveChangesAsync();

                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UserDbContext>>();
                        logger.LogInformation("Database initialized with seed data");
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<UserDbContext>>();
                logger.LogError(ex, "Error initializing database");
                throw;
            }
        }
    }
}
