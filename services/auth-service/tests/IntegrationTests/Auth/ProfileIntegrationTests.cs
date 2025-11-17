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
/// Integration tests for user profile endpoint
/// 
/// Tests profile retrieval with:
/// 1. Valid user lookup
/// 2. User data from database
/// 3. Proper response format
/// 4. Not found scenarios
/// </summary>
public class ProfileIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProfileIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_WithValidUserId_ShouldReturn200_AndUserData()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "profile.valid@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Profile",
            LastName = "User"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.ReadAsAsync<dynamic>();
        Assert.NotNull(profile);
        Assert.Equal("profile.valid@techbirdsfly.com", (string)profile.Email);
        Assert.Equal("Profile", (string)profile.FirstName);
        Assert.Equal("User", (string)profile.LastName);
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
    public async Task GetProfile_ShouldReturnCorrectFields()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "profile.fields@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Profile",
            LastName = "Fields"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{userId}");
        var profile = await response.ReadAsAsync<dynamic>();

        // Assert
        Assert.NotNull(profile.Id);
        Assert.NotNull(profile.Email);
        Assert.NotNull(profile.FirstName);
        Assert.NotNull(profile.LastName);
        Assert.NotNull(profile.IsEmailConfirmed);
        Assert.NotNull(profile.IsActive);
        Assert.NotNull(profile.CreatedAt);
    }

    [Fact]
    public async Task GetProfile_UserShouldBeActive_OnRegistration()
    {
        // Arrange - Register user
        var registerRequest = new RegisterRequestDto
        {
            Email = "profile.active@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Profile",
            LastName = "Active"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerBody = await registerResponse.ReadAsAsync<dynamic>();
        string userId = (string)registerBody.UserId;

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{userId}");
        var profile = await response.ReadAsAsync<dynamic>();

        // Assert
        Assert.True((bool)profile.IsActive);
    }

    [Fact]
    public async Task GetProfile_WithEmptyGuid_ShouldReturn404()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{emptyGuid}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
