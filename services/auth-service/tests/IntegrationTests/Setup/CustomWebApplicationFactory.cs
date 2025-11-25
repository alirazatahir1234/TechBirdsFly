using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.WebAPI;
using TechBirdsFly.Shared.Events.Contracts;

namespace AuthService.IntegrationTests.Setup;

public class CustomWebApplicationFactory 
    : WebApplicationFactory<Program>
{
    static CustomWebApplicationFactory()
    {
        // CRITICAL: Set environment BEFORE WebApplicationFactory tries to load Program
        // Use Process target so it's visible to reflection-based Program resolution
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test", EnvironmentVariableTarget.Process);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.UseTestServer();

        builder.ConfigureServices(services =>
        {
            // Remove real EF DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>)
            );

            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory SQLite
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlite("Data Source=:memory:");
            });

            // Mock ICacheService
            var mockCache = new Mock<ICacheService>();
            mockCache
                .Setup(x => x.GetAsync<object>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);
            mockCache
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            mockCache
                .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            services.AddSingleton(mockCache.Object);

            // Mock IEventPublisher
            var mockPublisher = new Mock<IEventPublisher>();
            mockPublisher
                .Setup(x => x.PublishEventAsync(It.IsAny<IEventContract>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            services.AddSingleton(mockPublisher.Object);
        });

        // Configure database after services are set up
        builder.Configure(app =>
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
            }
        });
    }

    public new HttpClient CreateClient()
    {
        return base.CreateClient();
    }
}
