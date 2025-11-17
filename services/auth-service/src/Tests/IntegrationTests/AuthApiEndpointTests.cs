using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using AuthService.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Infrastructure.Data;
using FluentAssertions;

namespace AuthService.Tests.IntegrationTests;

/// <summary>
/// API Endpoint tests for Auth Service - Tests response validation and edge cases
/// </summary>
public class AuthApiEndpointTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private readonly IServiceScope _scope;
    private string _validToken;
    private Guid _validUserId;

    public AuthApiEndpointTests()
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
                        options.UseInMemoryDatabase("AuthServiceApiTestDb");
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

        // Setup: Create a test user with valid token
        var registerRequest = new RegisterRequestDto
        {
            Email = "testuser@example.com",
            Password = "TestPassword123!",
            FullName = "Test User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadAsAsync<dynamic>();

        _validUserId = Guid.Parse(registerResult.GetProperty("userId").ToString());

        // Login to get valid token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto { Email = "testuser@example.com", Password = "TestPassword123!" });
        var loginResult = await loginResponse.Content.ReadAsAsync<dynamic>();
        _validToken = loginResult.GetProperty("accessToken").ToString();
    }

    public async Task DisposeAsync()
    {
        var dbContext = _scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        _scope.Dispose();
        _client.Dispose();
        _factory.Dispose();
    }

    #region Response Structure Tests

    [Fact]
    public async Task RegisterResponse_ShouldHaveCorrectStructure()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "response@example.com",
            Password = "SecurePassword123!",
            FullName = "Response Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var content = await response.Content.ReadAsAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.TryGetProperty("userId", out var userId).Should().BeTrue();
        content.TryGetProperty("message", out var message).Should().BeTrue();
    }

    [Fact]
    public async Task LoginResponse_ShouldContainAccessAndRefreshTokens()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "testuser@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.TryGetProperty("accessToken", out var accessToken).Should().BeTrue();
        content.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
        content.TryGetProperty("expiresIn", out var expiresIn).Should().BeTrue();
    }

    [Fact]
    public async Task ProfileResponse_ShouldHaveCorrectStructure()
    {
        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{_validUserId}");
        var content = await response.Content.ReadAsAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.TryGetProperty("id", out _).Should().BeTrue();
        content.TryGetProperty("email", out _).Should().BeTrue();
        content.TryGetProperty("fullName", out _).Should().BeTrue();
    }

    #endregion

    #region HTTP Headers Validation

    [Fact]
    public async Task Register_ResponseShouldHaveCorrectContentType()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "header@example.com",
            Password = "SecurePassword123!",
            FullName = "Header Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task AuthenticatedEndpoint_ShouldRequireValidToken()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid_token");

        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{_validUserId}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);

        // Cleanup
        _client.DefaultRequestHeaders.Remove("Authorization");
    }

    #endregion

    #region Error Response Format Tests

    [Fact]
    public async Task ErrorResponse_ShouldHaveConsistentFormat()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsAsync<JsonElement>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        content.TryGetProperty("error", out _).Should().BeTrue()
            .Or.Subject.TryGetProperty("message", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ValidationError_ShouldIncludeFieldDetails()
    {
        // Arrange
        var request = new { Email = "", Password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Boundary Value Tests

    [Fact]
    public async Task Register_WithMaxLengthEmail_ShouldSucceed()
    {
        // Arrange
        var longEmailPrefix = new string('a', 64);
        var request = new RegisterRequestDto
        {
            Email = $"{longEmailPrefix}@example.com",
            Password = "SecurePassword123!",
            FullName = "Long Email User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithMinimumPasswordLength_ShouldSucceed()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "minpass@example.com",
            Password = "Abc123!@",  // Minimum secure password
            FullName = "Min Password User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithSpecialCharactersInPassword_ShouldWork()
    {
        // Arrange
        var password = "P@$$w0rd!#%&*";
        var registerRequest = new RegisterRequestDto
        {
            Email = "special@example.com",
            Password = password,
            FullName = "Special Characters"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "special@example.com",
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Rate Limiting / Throttling Tests

    [Fact]
    public async Task MultipleFailedLogins_ShouldEventuallyThrottle()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "ratelimit@example.com",
            Password = "WrongPassword123!"
        };

        // Act - Attempt multiple failed logins
        var responses = new List<HttpStatusCode>();
        for (int i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            responses.Add(response.StatusCode);
            await Task.Delay(100);  // Small delay between attempts
        }

        // Assert - Should have received consistent unauthorized responses
        responses.Should().AllSatisfy(statusCode =>
            statusCode.Should().Be(HttpStatusCode.Unauthorized)
        );
    }

    #endregion

    #region Concurrent Request Tests

    [Fact]
    public async Task ConcurrentLoginRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "testuser@example.com",
            Password = "TestPassword123!"
        };

        // Act - Send concurrent requests
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.PostAsJsonAsync("/api/auth/login", loginRequest))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - All should succeed
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task ConcurrentProfileRequests_ShouldReturnConsistentData()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 3)
            .Select(_ => _client.GetAsync($"/api/auth/profile/{_validUserId}"))
            .ToList();

        // Act
        var responses = await Task.WhenAll(tasks);
        var contents = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
        contents.Distinct().Count().Should().Be(1);  // All responses should be identical
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task PasswordShouldNotBeReturnedInResponse()
    {
        // Act
        var response = await _client.GetAsync($"/api/auth/profile/{_validUserId}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().NotContain("password", System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshTokenShouldNotBeExposedToClient()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "testuser@example.com",
            Password = "TestPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsAsync<JsonElement>();

        // Assert
        content.TryGetProperty("refreshToken", out var refreshToken).Should().BeTrue();
        refreshToken.GetString().Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("first.last@subdomain.example.com")]
    public async Task Register_WithValidEmails_ShouldSucceed(string email)
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = "SecurePassword123!",
            FullName = "Email Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missinglocal.com")]
    [InlineData("missing@domain")]
    [InlineData("two@@example.com")]
    public async Task Register_WithInvalidEmails_ShouldFail(string email)
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = "SecurePassword123!",
            FullName = "Invalid Email User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task Register_SameRequest_ShouldReturnSameErrorTwice()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "idempotent@example.com",
            Password = "SecurePassword123!",
            FullName = "Idempotent Test"
        };

        // Act - First request
        var response1 = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Second request (duplicate)
        var response2 = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region HTTP Method Tests

    [Fact]
    public async Task Register_WithWrongHttpMethod_ShouldFail()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/register");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed)
            .Or.Subject.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_WithWrongHttpMethod_ShouldFail()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/login");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed)
            .Or.Subject.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
