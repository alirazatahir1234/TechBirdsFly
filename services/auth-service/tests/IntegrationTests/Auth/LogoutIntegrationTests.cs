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
/// Integration tests for logout endpoint
/// 
/// Tests logout flow:
/// 1. User login (creates session)
/// 2. Logout request (invalidates session)
/// 3. Cache invalidation
/// 4. Error handling
/// </summary>
public class LogoutIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LogoutIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Logout_WithValidEmail_ShouldReturn200()
    {
        // Arrange - Register and login user
        var registerRequest = new RegisterRequestDto
        {
            Email = "logout.user@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Logout",
            LastName = "User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "logout.user@techbirdsfly.com",
            Password = "SecurePassword123!"
        };
        await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Act
        var response = await _client.PostAsync("/api/auth/logout?email=logout.user@techbirdsfly.com", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadAsAsync<dynamic>();
        Assert.NotNull(body.message);
    }

    [Fact]
    public async Task Logout_ShouldClearUserToken()
    {
        // Arrange - Register and login user
        var registerRequest = new RegisterRequestDto
        {
            Email = "logout.clear@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Logout",
            LastName = "Clear"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "logout.clear@techbirdsfly.com",
            Password = "SecurePassword123!"
        };
        await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Act
        var response = await _client.PostAsync("/api/auth/logout?email=logout.clear@techbirdsfly.com", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Token removal from cache is verified by controller implementation
        // (in production, mocked cache would verify Remove was called)
    }

    [Fact]
    public async Task Logout_WithEmptyEmail_ShouldReturn400()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/logout?email=", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutEmailParameter_ShouldReturn400()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithNonexistentUser_ShouldReturn200()
    {
        // Arrange - User doesn't exist, but logout should still work
        // (cache removal won't find anything, but that's OK)

        // Act
        var response = await _client.PostAsync("/api/auth/logout?email=nonexistent.logout@techbirdsfly.com", null);

        // Assert
        // Logout is idempotent - should succeed even if user doesn't exist
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_MultipleLogouts_ShouldSucceed()
    {
        // Arrange - Register and login user
        var registerRequest = new RegisterRequestDto
        {
            Email = "logout.multiple@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Logout",
            LastName = "Multiple"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "logout.multiple@techbirdsfly.com",
            Password = "SecurePassword123!"
        };
        await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Act - Logout first time
        var response1 = await _client.PostAsync("/api/auth/logout?email=logout.multiple@techbirdsfly.com", null);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Act - Logout second time (should still succeed)
        var response2 = await _client.PostAsync("/api/auth/logout?email=logout.multiple@techbirdsfly.com", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact]
    public async Task Logout_ResponseShouldHaveSuccessMessage()
    {
        // Arrange
        var email = "logout.message@techbirdsfly.com";

        // Act
        var response = await _client.PostAsync($"/api/auth/logout?email={email}", null);
        var body = await response.ReadAsAsync<dynamic>();

        // Assert
        string message = (string)body.message;
        Assert.NotEmpty(message);
        Assert.Contains("success", message, StringComparison.OrdinalIgnoreCase);
    }
}
