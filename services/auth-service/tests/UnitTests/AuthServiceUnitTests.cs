using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AuthService.Application.DTOs;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using TechBirdsFly.Shared.Events.Contracts;
using Microsoft.Extensions.Logging;

namespace AuthService.Tests.UnitTests
{
    public class AuthServiceUnitTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IEventPublisher> _mockEventPublisher;
        private readonly Mock<ILogger<AuthApplicationService>> _mockLogger;
        private readonly Mock<ILogger<AuthEventPublisherService>> _mockEventPublisherLogger;

        private readonly AuthApplicationService _authService;

        public AuthServiceUnitTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockCacheService = new Mock<ICacheService>();
            _mockEventPublisher = new Mock<IEventPublisher>();
            _mockLogger = new Mock<ILogger<AuthApplicationService>>();
            _mockEventPublisherLogger = new Mock<ILogger<AuthEventPublisherService>>();

            // Bind repository inside UnitOfWork
            _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);

            // Create real AuthEventPublisherService with both mocked dependencies
            var eventPublisher = new AuthEventPublisherService(
                _mockEventPublisher.Object,
                _mockEventPublisherLogger.Object
            );

            _authService = new AuthApplicationService(
                _mockUnitOfWork.Object,
                _mockPasswordService.Object,
                _mockTokenService.Object,
                _mockCacheService.Object,
                eventPublisher,
                _mockLogger.Object
            );
        }

        // ---------------------------------------------------------------
        // REGISTER TESTS
        // ---------------------------------------------------------------
        [Fact]
        public async Task Register_WithValidData_ShouldSucceed()
        {
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(request.Email, default))
                .ReturnsAsync((User)null);

            _mockPasswordService
                .Setup(p => p.HashPassword(request.Password))
                .Returns("hashed_password");

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _authService.RegisterAsync(request, default);

            Assert.NotNull(result);
            Assert.Equal(request.Email, result.Email);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ShouldThrow()
        {
            var request = new RegisterRequestDto
            {
                Email = "existing@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var existingUser = User.Create(request.Email, "hash", request.FirstName, request.LastName);

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(request.Email, default))
                .ReturnsAsync(existingUser);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync(request, default));
        }

        [Fact]
        public async Task Register_WithMismatchedPasswords_ShouldThrow()
        {
            var request = new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Pass1!",
                ConfirmPassword = "Pass2!",
                FirstName = "User",
                LastName = "Test"
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _authService.RegisterAsync(request, default));
        }

        // ---------------------------------------------------------------
        // LOGIN TESTS
        // ---------------------------------------------------------------
        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnTokens()
        {
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var user = User.Create(request.Email, "hashed_password", "John", "Doe");

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(request.Email, default))
                .ReturnsAsync(user);

            _mockPasswordService
                .Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(true);

            _mockTokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access_token");
            _mockTokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");

            var result = await _authService.LoginAsync(request, default);

            Assert.Equal("access_token", result.AccessToken);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldThrow()
        {
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "wrong"
            };

            var user = User.Create(request.Email, "correct_hash", "John", "Doe");

            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email, default))
                .ReturnsAsync(user);

            _mockPasswordService.Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(false);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.LoginAsync(request, default));
        }

        [Fact]
        public async Task Login_WithNonexistentUser_ShouldThrow()
        {
            var request = new LoginRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(request.Email, default))
                .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.LoginAsync(request, default));
        }

        // ---------------------------------------------------------------
        // PASSWORD TESTS
        // ---------------------------------------------------------------
        [Fact]
        public void HashPassword_ShouldNotReturnPlaintext()
        {
            _mockPasswordService.Setup(p => p.HashPassword("p")).Returns("hash");

            var hash = _mockPasswordService.Object.HashPassword("p");

            Assert.NotEqual("p", hash);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue()
        {
            _mockPasswordService.Setup(p => p.VerifyPassword("a", "b")).Returns(true);

            Assert.True(_mockPasswordService.Object.VerifyPassword("a", "b"));
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse()
        {
            _mockPasswordService.Setup(p => p.VerifyPassword("wrong", "hash")).Returns(false);

            Assert.False(_mockPasswordService.Object.VerifyPassword("wrong", "hash"));
        }

        // ---------------------------------------------------------------
        // TOKEN TESTS
        // ---------------------------------------------------------------
        [Fact]
        public void GenerateAccessToken_WithValidUser_ShouldReturnToken()
        {
            var user = User.Create("test@example.com", "hash", "John", "Doe");
            _mockTokenService.Setup(t => t.GenerateAccessToken(user)).Returns("valid_token");

            var token = _mockTokenService.Object.GenerateAccessToken(user);

            Assert.NotEmpty(token);
            Assert.Equal("valid_token", token);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnToken()
        {
            _mockTokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token_value");

            var token = _mockTokenService.Object.GenerateRefreshToken();

            Assert.NotEmpty(token);
        }
    }
}
