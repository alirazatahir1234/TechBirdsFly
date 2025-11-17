using Xunit;
using Moq;
using AuthService.Application.DTOs;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Tests.UnitTests;

/// <summary>
/// Unit tests for Auth Service
/// Tests authentication logic, JWT token generation, and user operations
/// </summary>
public class AuthServiceUnitTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<AuthApplicationService>> _mockLogger;
    private readonly AuthApplicationService _authService;

    public AuthServiceUnitTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<AuthApplicationService>>();
        
        _authService = new AuthApplicationService(
            _mockUserRepository.Object,
            _mockCacheService.Object,
            _mockLogger.Object
        );
    }

    #region Registration Tests

    [Fact]
    public async Task Register_WithValidRequest_ShouldCreateNewUser()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FullName = "Test User"
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        _mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = Guid.NewGuid(), Email = request.Email, FullName = request.FullName });

        // Act
        var result = await _authService.RegisterAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.FullName, result.FullName);
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "existing@example.com",
            Password = "SecurePassword123!",
            FullName = "Test User"
        };

        var existingUser = new User { UserId = Guid.NewGuid(), Email = request.Email };
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.RegisterAsync(request, CancellationToken.None)
        );
        Assert.Contains("email already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "invalid-email",
            Password = "SecurePassword123!",
            FullName = "Test User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _authService.RegisterAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldThrowException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "weak",  // Too short and weak
            FullName = "Test User"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _authService.RegisterAsync(request, CancellationToken.None)
        );
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var email = "test@example.com";
        var password = "SecurePassword123!";
        var passwordHash = HashPassword(password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsEmailConfirmed = true,
            IsActive = true
        };

        var request = new LoginRequestDto { Email = email, Password = password };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var email = "test@example.com";
        var correctPassword = "SecurePassword123!";
        var wrongPassword = "WrongPassword123!";
        var passwordHash = HashPassword(correctPassword);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsEmailConfirmed = true,
            IsActive = true
        };

        var request = new LoginRequestDto { Email = email, Password = wrongPassword };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authService.LoginAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ShouldThrowException()
    {
        // Arrange
        var request = new LoginRequestDto { Email = "nonexistent@example.com", Password = "Password123!" };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authService.LoginAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_ShouldThrowException()
    {
        // Arrange
        var email = "test@example.com";
        var password = "SecurePassword123!";
        var passwordHash = HashPassword(password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsEmailConfirmed = false,  // Not confirmed
            IsActive = true
        };

        var request = new LoginRequestDto { Email = email, Password = password };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.LoginAsync(request, CancellationToken.None)
        );
        Assert.Contains("email not confirmed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldThrowException()
    {
        // Arrange
        var email = "test@example.com";
        var password = "SecurePassword123!";
        var passwordHash = HashPassword(password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsEmailConfirmed = true,
            IsActive = false  // Inactive
        };

        var request = new LoginRequestDto { Email = email, Password = password };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.LoginAsync(request, CancellationToken.None)
        );
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task GetProfile_WithValidUserId_ShouldReturnUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Email = "test@example.com",
            FullName = "Test User",
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.GetProfileAsync(userId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetProfile_WithNonexistentUserId_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _authService.GetProfileAsync(userId, CancellationToken.None)
        );
    }

    #endregion

    #region Email Confirmation Tests

    [Fact]
    public async Task ConfirmEmail_WithValidUserId_ShouldConfirmEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId, IsEmailConfirmed = false };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = userId, IsEmailConfirmed = true });

        // Act
        await _authService.ConfirmEmailAsync(userId, CancellationToken.None);

        // Assert
        _mockUserRepository.Verify(
            r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ConfirmEmail_AlreadyConfirmed_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId, IsEmailConfirmed = true };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.ConfirmEmailAsync(userId, CancellationToken.None)
        );
    }

    #endregion

    #region Helper Methods

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }

    #endregion
}
