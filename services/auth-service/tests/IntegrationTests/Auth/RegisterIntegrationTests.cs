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
/// Integration tests for user registration endpoint
/// 
/// Tests the complete flow:
/// 1. HTTP request through routing
/// 2. Controller receives and validates request
/// 3. AuthApplicationService processes registration
/// 4. User is created in database
/// 5. Response is serialized and returned
/// 
/// Uses real database (in-memory SQLite) and real repositories
/// but mocks external dependencies (cache, event publisher)
/// </summary>
[Collection("Integration Tests")]
public class RegisterIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser_AndReturn200()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "valid.user@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Ali",
            LastName = "Raza"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.ReadAsAsync<dynamic>();
        Assert.NotNull(body);
        Assert.NotNull(body.UserId);
        Assert.Equal("valid.user@techbirdsfly.com", (string)body.Email);
        Assert.Contains("registered successfully", (string)body.Message);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldStoreInDatabase()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "database.user@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Database",
            LastName = "User"
        };

        // Act - Register first time
        var response1 = await _client.PostAsJsonAsync("/api/auth/register", request);
        var body1 = await response1.ReadAsAsync<dynamic>();
        string userId = (string)body1.UserId;

        // Act - Retrieve profile to verify stored data
        var response2 = await _client.GetAsync($"/api/auth/profile/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var profile = await response2.ReadAsAsync<dynamic>();
        Assert.Equal("database.user@techbirdsfly.com", (string)profile.Email);
        Assert.Equal("Database", (string)profile.FirstName);
        Assert.Equal("User", (string)profile.LastName);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn400()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "duplicate@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "First",
            LastName = "User"
        };

        // Act - Register first time
        var response1 = await _client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Act - Try to register with same email
        var response2 = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        var body = await response2.ReadAsAsync<dynamic>();
        Assert.NotNull(body.error);
        Assert.Contains("already exists", (string)body.error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturn400()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "mismatch@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "DifferentPassword456!",
            FirstName = "Ali",
            LastName = "Raza"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.ReadAsAsync<dynamic>();
        Assert.NotNull(body.error);
    }

    [Fact]
    public async Task Register_WithEmptyEmail_ShouldReturn400()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Ali",
            LastName = "Raza"
        };

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
            Email = "weak@techbirdsfly.com",
            Password = "weak",
            ConfirmPassword = "weak",
            FirstName = "Ali",
            LastName = "Raza"
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
            Email = "invalid-email",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Ali",
            LastName = "Raza"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_UserIdShouldBeGuid()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "guid.test@techbirdsfly.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Ali",
            LastName = "Raza"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var body = await response.ReadAsAsync<dynamic>();
        string userIdStr = (string)body.UserId;

        // Assert
        Assert.True(Guid.TryParse(userIdStr, out var userId));
        Assert.NotEqual(Guid.Empty, userId);
    }
}
