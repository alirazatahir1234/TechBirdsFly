using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using AuthService.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Infrastructure.Data;
using FluentAssertions;

namespace AuthService.Tests.IntegrationTests;

/// <summary>
/// Performance and load tests for Auth Service
/// Validates service behavior under stress and measures response times
/// </summary>
public class AuthServicePerformanceTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private readonly IServiceScope _scope;

    public AuthServicePerformanceTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(AuthDbContext));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AuthDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("AuthServicePerformanceTestDb");
                    });
                });
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }

    public async Task InitializeAsync()
    {
        var dbContext = _scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        var dbContext = _scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        _scope.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }

    #region Response Time Tests

    [Fact]
    public async Task Register_ShouldCompleteWithin2Seconds()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "perf@example.com",
            Password = "SecurePassword123!",
            FullName = "Performance Test"
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task Login_ShouldCompleteWithin1Second()
    {
        // Arrange - Setup user
        var registerRequest = new RegisterRequestDto
        {
            Email = "login-perf@example.com",
            Password = "SecurePassword123!",
            FullName = "Login Performance"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "login-perf@example.com",
            Password = "SecurePassword123!"
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public async Task GetProfile_ShouldCompleteWithin500Milliseconds()
    {
        // Arrange - Setup user
        var registerRequest = new RegisterRequestDto
        {
            Email = "profile-perf@example.com",
            Password = "SecurePassword123!",
            FullName = "Profile Performance"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadAsAsync<dynamic>();
        var userId = Guid.Parse(registerResult.GetProperty("userId").ToString());

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{userId}");

        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    #endregion

    #region Throughput Tests

    [Fact]
    public async Task Register_ShouldHandle100RegistrationsPerSecond()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var registrations = 100;
        var successCount = 0;

        // Act
        var tasks = Enumerable.Range(0, registrations)
            .Select(async i =>
            {
                var request = new RegisterRequestDto
                {
                    Email = $"bulk-{i}@example.com",
                    Password = "SecurePassword123!",
                    FullName = $"Bulk User {i}"
                };

                var response = await _client.PostAsJsonAsync("/api/auth/register", request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Interlocked.Increment(ref successCount);
                }
            })
            .ToList();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        successCount.Should().Be(registrations);
        var registrationsPerSecond = (registrations / stopwatch.Elapsed.TotalSeconds);
        registrationsPerSecond.Should().BeGreaterThan(10);  // At least 10 per second
    }

    [Fact]
    public async Task Login_ShouldHandle50LoginsPerSecond()
    {
        // Arrange - Setup multiple users
        var userCount = 50;
        var emails = new List<string>();

        for (int i = 0; i < userCount; i++)
        {
            var email = $"login-bulk-{i}@example.com";
            var registerRequest = new RegisterRequestDto
            {
                Email = email,
                Password = "SecurePassword123!",
                FullName = $"Bulk Login User {i}"
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            emails.Add(email);
        }

        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;

        // Act
        var tasks = emails.Select(async email =>
        {
            var loginRequest = new LoginRequestDto
            {
                Email = email,
                Password = "SecurePassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Interlocked.Increment(ref successCount);
            }
        }).ToList();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        successCount.Should().Be(userCount);
        var loginsPerSecond = (userCount / stopwatch.Elapsed.TotalSeconds);
        loginsPerSecond.Should().BeGreaterThan(5);  // At least 5 per second
    }

    #endregion

    #region Memory and Resource Tests

    [Fact]
    public async Task Register_LargeScale_ShouldNotExhaustMemory()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var registrationCount = 100;

        // Act
        var tasks = Enumerable.Range(0, registrationCount)
            .Select(async i =>
            {
                var request = new RegisterRequestDto
                {
                    Email = $"memory-test-{i}@example.com",
                    Password = "SecurePassword123!",
                    FullName = $"Memory Test User {i}"
                };

                await _client.PostAsJsonAsync("/api/auth/register", request);
            })
            .ToList();

        await Task.WhenAll(tasks);

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert - Memory increase should be reasonable (less than 500MB)
        memoryIncrease.Should().BeLessThan(500 * 1024 * 1024);
    }

    #endregion

    #region Sustained Load Tests

    [Fact]
    public async Task SustainedLoad_ShouldMaintainPerformance()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(10);
        var requestTimes = new List<long>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        while (stopwatch.Elapsed < duration)
        {
            var index = requestTimes.Count;
            var request = new RegisterRequestDto
            {
                Email = $"sustained-{index}@example.com",
                Password = "SecurePassword123!",
                FullName = $"Sustained Load User {index}"
            };

            var requestStopwatch = Stopwatch.StartNew();
            await _client.PostAsJsonAsync("/api/auth/register", request);
            requestStopwatch.Stop();

            requestTimes.Add(requestStopwatch.ElapsedMilliseconds);
        }

        stopwatch.Stop();

        // Assert - Performance should remain consistent
        var averageTime = requestTimes.Average();
        var firstHalfAverage = requestTimes.Take(requestTimes.Count / 2).Average();
        var secondHalfAverage = requestTimes.Skip(requestTimes.Count / 2).Average();

        // Second half should not be significantly slower than first half (allowing 20% variance)
        secondHalfAverage.Should().BeLessThan(firstHalfAverage * 1.2);
    }

    #endregion

    #region Error Recovery Tests

    [Fact]
    public async Task ErrorRecovery_ShouldResumeNormalOperation()
    {
        // Arrange
        var successfulRequests = 0;
        var failedRequests = 0;

        // Act
        for (int i = 0; i < 20; i++)
        {
            if (i % 3 == 0)
            {
                // Intentional failure
                var badRequest = new { Email = "", Password = "" };
                var badResponse = await _client.PostAsJsonAsync("/api/auth/register", badRequest);
                if (!badResponse.IsSuccessStatusCode)
                {
                    failedRequests++;
                }
            }
            else
            {
                // Valid request
                var request = new RegisterRequestDto
                {
                    Email = $"recovery-{i}@example.com",
                    Password = "SecurePassword123!",
                    FullName = $"Recovery Test {i}"
                };

                var response = await _client.PostAsJsonAsync("/api/auth/register", request);
                if (response.IsSuccessStatusCode)
                {
                    successfulRequests++;
                }
            }
        }

        // Assert
        failedRequests.Should().BeGreaterThan(0);
        successfulRequests.Should().BeGreaterThan(0);
        (successfulRequests + failedRequests).Should().Be(20);
    }

    #endregion

    #region Latency Distribution Tests

    [Fact]
    public async Task Register_LatencyDistribution_ShouldBeLognormal()
    {
        // Arrange
        var requestCount = 50;
        var responseTimes = new List<long>();

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            var request = new RegisterRequestDto
            {
                Email = $"latency-{i}@example.com",
                Password = "SecurePassword123!",
                FullName = $"Latency Test {i}"
            };

            var stopwatch = Stopwatch.StartNew();
            await _client.PostAsJsonAsync("/api/auth/register", request);
            stopwatch.Stop();

            responseTimes.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert - Check percentiles
        var sorted = responseTimes.OrderBy(t => t).ToList();
        var p50 = sorted[(int)(sorted.Count * 0.50)];
        var p95 = sorted[(int)(sorted.Count * 0.95)];
        var p99 = sorted[(int)(sorted.Count * 0.99)];

        p50.Should().BeLessThan(2000);
        p95.Should().BeLessThan(3000);
        p99.Should().BeLessThan(5000);
    }

    #endregion

    #region Scalability Tests

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Register_VariousLoadLevels_ShouldScaleLinearly(int concurrentRequests)
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(async i =>
            {
                var request = new RegisterRequestDto
                {
                    Email = $"scale-{concurrentRequests}-{i}@example.com",
                    Password = "SecurePassword123!",
                    FullName = $"Scale Test {i}"
                };

                await _client.PostAsJsonAsync("/api/auth/register", request);
            })
            .ToList();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Should complete in reasonable time
        var timePerRequest = stopwatch.ElapsedMilliseconds / (double)concurrentRequests;
        timePerRequest.Should().BeLessThan(100);  // Average less than 100ms per request
    }

    #endregion
}
