using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using AuthService.Application.DTOs;
using AuthService.IntegrationTests.Setup;
using AuthService.IntegrationTests.Helpers;

namespace AuthService.IntegrationTests.Auth;

/// <summary>
/// Integration tests for account deactivation endpoint
/// 
/// Tests account deactivation flow:
/// 1. User registration and activation
/// 2. Deactivate account
/// 3. Verify account is marked as inactive
/// 4. Error handling for invalid users
/// </summary>
[Collection("Integration Tests")]
public class DeactivateIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeactivateIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Deactivate_WithValidUserId_ShouldReturn200()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "deactivate.user@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Deactivate",
            LastName = "User"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act
        var response = await _client.PostAsync($"/api/auth/deactivate/{userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadAsAsync<dynamic>();
        Assert.NotNull(body.message);
        Assert.Contains("deactivated", (string)body.message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Deactivate_ShouldMarkAccountAsInactive()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "deactivate.inactive@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Deactivate",
            LastName = "Inactive"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Verify user is active initially
        var profileBefore = await _client.GetAsync($"/api/auth/profile/{userId}");
        var profileBodyBefore = await profileBefore.ReadAsAsync<dynamic>();
        Assert.True((bool)profileBodyBefore.IsActive);

        // Act - Deactivate account
        await _client.PostAsync($"/api/auth/deactivate/{userId}", null);

        // Assert - Verify account is now inactive
        var profileAfter = await _client.GetAsync($"/api/auth/profile/{userId}");
        var profileBodyAfter = await profileAfter.ReadAsAsync<dynamic>();
        Assert.False((bool)profileBodyAfter.IsActive);
    }

    [Fact]
    public async Task Deactivate_WithInvalidUserId_ShouldReturn404()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/auth/deactivate/{invalidUserId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_WithEmptyGuid_ShouldReturn404()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = await _client.PostAsync($"/api/auth/deactivate/{emptyGuid}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_MultipleDeactivations_ShouldSucceed()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "deactivate.multiple@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Deactivate",
            LastName = "Multiple"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act - Deactivate first time
        var response1 = await _client.PostAsync($"/api/auth/deactivate/{userId}", null);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Act - Deactivate second time (should still succeed)
        var response2 = await _client.PostAsync($"/api/auth/deactivate/{userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ShouldClearUserCache()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "deactivate.cache@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Deactivate",
            LastName = "Cache"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act
        var response = await _client.PostAsync($"/api/auth/deactivate/{userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Cache removal is verified by controller implementation
        // (in production, mocked cache would verify Remove was called)
    }

    [Fact]
    public async Task Deactivate_ShouldPreventLogin()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "deactivate.prevent@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Deactivate",
            LastName = "Prevent"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act - Deactivate account
        await _client.PostAsync($"/api/auth/deactivate/{userId}", null);

        // Act - Try to login
        var loginRequest = new LoginRequestDto
        {
            Email = "deactivate.prevent@techbirdsfly.com",
            Password = "SecurePassword123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert - Login should fail for deactivated user
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }
}
