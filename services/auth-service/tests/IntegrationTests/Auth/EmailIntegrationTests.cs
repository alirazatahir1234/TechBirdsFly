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
/// Integration tests for email confirmation endpoint
/// 
/// Tests email confirmation flow:
/// 1. User registration (unconfirmed email)
/// 2. Email confirmation
/// 3. Verify email is marked as confirmed
/// 4. Error handling for invalid users
/// </summary>
public class EmailIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmailIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ConfirmEmail_WithValidUserId_ShouldReturn200()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "confirm.email@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Email",
            LastName = "Confirm"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act
        var response = await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadAsAsync<dynamic>();
        Assert.NotNull(body.message);
        Assert.Contains("confirmed", (string)body.message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldMarkEmailAsConfirmed()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "confirm.verified@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Email",
            LastName = "Verified"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Verify email is not confirmed initially
        var profileBefore = await _client.GetAsync($"/api/auth/profile/{userId}");
        var profileBodyBefore = await profileBefore.ReadAsAsync<dynamic>();
        Assert.False((bool)profileBodyBefore.IsEmailConfirmed);

        // Act - Confirm email
        await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);

        // Assert - Verify email is now confirmed
        var profileAfter = await _client.GetAsync($"/api/auth/profile/{userId}");
        var profileBodyAfter = await profileAfter.ReadAsAsync<dynamic>();
        Assert.True((bool)profileBodyAfter.IsEmailConfirmed);
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
    public async Task ConfirmEmail_WithEmptyGuid_ShouldReturn404()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = await _client.PostAsync($"/api/auth/confirm-email/{emptyGuid}", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_MultipleConfirmations_ShouldSucceed()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "confirm.multiple@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Email",
            LastName = "Multiple"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act - Confirm first time
        var response1 = await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Act - Confirm second time (should still succeed)
        var response2 = await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_ResponseShouldHaveSuccessMessage()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "confirm.message@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Email",
            LastName = "Message"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act
        var response = await _client.PostAsync($"/api/auth/confirm-email/{userId}", null);
        var body = await response.ReadAsAsync<dynamic>();

        // Assert
        string message = (string)body.message;
        Assert.NotEmpty(message);
        Assert.Contains("successfully", message, StringComparison.OrdinalIgnoreCase);
    }
}
