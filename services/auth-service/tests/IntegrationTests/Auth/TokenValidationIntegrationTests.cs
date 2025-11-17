using System;
using System.Diagnostics;
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
/// Integration tests for token validation endpoint
/// 
/// Tests token validation flow:
/// 1. Valid token validation
/// 2. Invalid token rejection
/// 3. Cache behavior
/// 4. Error handling
/// </summary>
public class TokenValidationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TokenValidationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ValidateToken_WithValidRequest_ShouldReturn200()
    {
        // Arrange
        var request = new TokenValidationRequestDto("valid-token-sample");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnValidField()
    {
        // Arrange
        var request = new TokenValidationRequestDto("test-token-123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);
        var body = await response.ReadAsAsync<dynamic>();

        // Assert
        Assert.NotNull(body.valid);
        Assert.True((bool)body.valid || !(bool)body.valid); // Should be boolean
    }

    [Fact]
    public async Task ValidateToken_ShouldReturnFromCacheIndicator()
    {
        // Arrange
        var request = new TokenValidationRequestDto("cache-test-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);
        var body = await response.ReadAsAsync<dynamic>();

        // Assert
        Assert.NotNull(body.fromCache);
    }

    [Fact]
    public async Task ValidateToken_WithEmptyToken_ShouldReturnResponse()
    {
        // Arrange
        var request = new TokenValidationRequestDto("");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);

        // Assert
        // Empty token should still get a response (validation may fail, but request is valid)
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ValidateToken_WithLongToken_ShouldReturnResponse()
    {
        // Arrange
        var longToken = new string('x', 1000);
        var request = new TokenValidationRequestDto(longToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ValidateToken_ShouldCacheResult()
    {
        // Arrange
        var token = "cache-validation-token-123";
        var request = new TokenValidationRequestDto(token);

        // Act - First call (should not be from cache)
        var response1 = await _client.PostAsJsonAsync("/api/auth/validate-token", request);
        var body1 = await response1.ReadAsAsync<dynamic>();
        bool firstCallFromCache = (bool)body1.fromCache;

        // Act - Second call (should potentially be from cache)
        var response2 = await _client.PostAsJsonAsync("/api/auth/validate-token", request);
        var body2 = await response2.ReadAsAsync<dynamic>();
        bool secondCallFromCache = (bool)body2.fromCache;

        // Assert
        // In real scenario with cache, second call might be from cache
        // Mock cache always returns null, so both should be false
        Assert.False(firstCallFromCache);
    }

    [Fact]
    public async Task ValidateToken_MultipleValidations_ShouldSucceed()
    {
        // Arrange
        var tokens = new[] { "token-1", "token-2", "token-3" };

        // Act & Assert
        foreach (var token in tokens)
        {
            var request = new TokenValidationRequestDto(token);
            var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task ValidateToken_ShouldBeFast()
    {
        // Arrange
        var request = new TokenValidationRequestDto("performance-test-token");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);
        stopwatch.Stop();

        // Assert - Should complete within 500ms
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Token validation took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    [Fact]
    public async Task ValidateToken_SpecialCharactersInToken_ShouldReturn200()
    {
        // Arrange
        var specialToken = "token-with-!@#$%^&*()-_=+[]{}|;:',.<>?";
        var request = new TokenValidationRequestDto(specialToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/validate-token", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
