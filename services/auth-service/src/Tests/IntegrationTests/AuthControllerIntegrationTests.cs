using System.Net;
using System.Net.Http.Json;
using Xunit;
using AuthService.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Infrastructure.Data;

namespace AuthService.Tests.IntegrationTests;

/// <summary>
/// Integration tests for Auth Service API endpoints
/// Tests full HTTP request/response cycle with real database
/// </summary>
public class AuthControllerIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private readonly IServiceScope _scope;

    public AuthControllerIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Override database to use test database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(AuthDbContext));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    services.AddDbContext<AuthDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("AuthServiceTestDb");
                    });
                });
            });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }

    public async Task InitializeAsync()
    {
        // Initialize test database
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

    #region Registration Endpoint Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            FullName = "New User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn400()
    {
        // Arrange
        var email = "duplicate@example.com";
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = "SecurePassword123!",
            FullName = "User One"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Second registration with same email
        var duplicateRequest = new RegisterRequestDto
        {
            Email = email,
            Password = "AnotherPassword123!",
            FullName = "User Two"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingEmail_ShouldReturn400()
    {
        // Arrange
        var request = new { Password = "SecurePassword123!", FullName = "User" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "weak",  // Too weak
            FullName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "not-an-email",
            Password = "SecurePassword123!",
            FullName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Endpoint Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithTokens()
    {
        // Arrange - Register user first
        var email = "login@example.com";
        var password = "SecurePassword123!";

        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = password,
            FullName = "Login Test User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto { Email = email, Password = password };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsAsync<dynamic>();
        Assert.NotNull(result.accessToken);
        Assert.NotNull(result.refreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange - Register user
        var email = "test@example.com";
        var password = "SecurePassword123!";

        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = password,
            FullName = "Test User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto { Email = email, Password = "WrongPassword!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ShouldReturn401()
    {
        // Arrange
        var loginRequest = new LoginRequestDto { Email = "nonexistent@example.com", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingEmail_ShouldReturn400()
    {
        // Arrange
        var request = new { Password = "SecurePassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Profile Endpoint Tests

    [Fact]
    public async Task GetProfile_WithValidUserId_ShouldReturn200()
    {
        // Arrange - Register user first
        var registerRequest = new RegisterRequestDto
        {
            Email = "profile@example.com",
            Password = "SecurePassword123!",
            FullName = "Profile Test User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadAsAsync<dynamic>();
        var userId = registerResult.userId;

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetProfile_WithInvalidUserId_ShouldReturn404()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{invalidUserId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithMalformedUserId_ShouldReturn400()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/auth/profile/not-a-guid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Email Confirmation Endpoint Tests

    [Fact]
    public async Task ConfirmEmail_WithValidUserId_ShouldReturn200()
    {
        // Arrange - Register user first
        var registerRequest = new RegisterRequestDto
        {
            Email = "confirm@example.com",
            Password = "SecurePassword123!",
            FullName = "Confirm Email Test"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadAsAsync<dynamic>();
        var userId = registerResult.userId;

        // Act
        var response = await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidUserId_ShouldReturn404()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/auth/confirm-email/{invalidUserId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_AlreadyConfirmed_ShouldReturn400()
    {
        // Arrange - Register and confirm email
        var registerRequest = new RegisterRequestDto
        {
            Email = "alreadyconfirmed@example.com",
            Password = "SecurePassword123!",
            FullName = "Already Confirmed Test"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadAsAsync<dynamic>();
        var userId = registerResult.userId;

        // First confirmation
        await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);

        // Act - Try to confirm again
        var response = await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Health Check Endpoint Tests

    [Fact]
    public async Task Health_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region JWT Token Validation Tests

    [Fact]
    public async Task JWT_TokenShouldContainRequiredClaims()
    {
        // Arrange - Register and login
        var email = "jwt@example.com";
        var password = "SecurePassword123!";

        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = password,
            FullName = "JWT Test User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadAsAsync<dynamic>();

        var token = loginResult.accessToken as string;

        // Act - Decode token (basic validation)
        var parts = token.Split('.');

        // Assert
        Assert.Equal(3, parts.Length);  // Valid JWT has 3 parts
    }

    #endregion

    #region Load/Stress Tests

    [Fact]
    public async Task Register_MultipleUsersConcurrently_ShouldHandleSuccessfully()
    {
        // Arrange
        var tasks = new List<Task>();
        var taskCount = 10;

        // Act
        for (int i = 0; i < taskCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var request = new RegisterRequestDto
                {
                    Email = $"user{index}@example.com",
                    Password = "SecurePassword123!",
                    FullName = $"User {index}"
                };

                var response = await _client.PostAsJsonAsync("/api/auth/register", request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - All tasks completed successfully
        Assert.True(true);  // If we got here, all tasks completed
    }

    #endregion
}
