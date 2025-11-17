using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Persistence;
using AuthService.WebAPI;
using TechBirdsFly.Shared.Events.Contracts;

namespace AuthService.IntegrationTests.Setup;

/// <summary>
/// Custom Web Application Factory for integration testing
/// 
/// Provides:
/// - In-memory SQLite database for isolation
/// - Mocked ICacheService (external dependency)
/// - Mocked IEventPublisher (external dependency)
/// - Real AuthApplicationService (service under test)
/// - Real EF Core repositories with real database context
/// - Real dependency injection container
/// 
/// This ensures tests use real controllers, routing, and business logic
/// while isolating external dependencies like cache and event bus.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Configure web host for testing environment
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ======================================================================
            // STEP 1: Remove production database context
            // ======================================================================
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>)
            );
            
            if (dbDescriptor != null)
            {
                services.Remove(dbDescriptor);
            }

            // ======================================================================
            // STEP 2: Add SQLite in-memory database
            // Supports relational features (foreign keys, constraints)
            // Automatically recreated for each test
            // ======================================================================
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlite("Filename=:memory:");
            });

            // ======================================================================
            // STEP 3: Replace ICacheService with mock
            // Prevents external cache dependency (Redis, Memcached, etc.)
            // ======================================================================
            var mockCacheService = new Mock<ICacheService>();
            
            // Default mock behavior: always return null for Gets, succeed for Sets/Removes
            mockCacheService
                .Setup(x => x.GetAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns((string _, CancellationToken __) => Task.FromResult<object?>(null));
            
            mockCacheService
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            mockCacheService
                .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            services.AddSingleton(mockCacheService.Object);

            // ======================================================================
            // STEP 4: Replace IEventPublisher with mock
            // Prevents external event bus dependency (RabbitMQ, Azure ServiceBus, etc.)
            // ======================================================================
            var mockEventPublisher = new Mock<IEventPublisher>();
            
            mockEventPublisher
                .Setup(x => x.PublishEventAsync(It.IsAny<IEventContract>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            services.AddSingleton(mockEventPublisher.Object);

            // ======================================================================
            // STEP 5: Build service provider for database initialization
            // ======================================================================
            var serviceProvider = services.BuildServiceProvider();

            // ======================================================================
            // STEP 6: Create database schema and initialize
            // This runs migrations and creates all tables in-memory
            // ======================================================================
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                
                // Open connection for SQLite in-memory database
                // Required to keep in-memory database alive during test
                db.Database.OpenConnection();
                
                // Create all database objects (tables, indexes, etc.)
                db.Database.EnsureCreated();
            }
        });
    }

    /// <summary>
    /// Create a new test client with clean database for each test
    /// </summary>
    public new HttpClient CreateClient()
    {
        return base.CreateClient();
    }
}
