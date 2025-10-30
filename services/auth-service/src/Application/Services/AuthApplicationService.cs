namespace AuthService.Application.Services;

using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

/// <summary>
/// Core authentication business logic service
/// Implements use cases for login, registration, token management, etc.
/// </summary>
public class AuthApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cacheService;

    public AuthApplicationService(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ITokenService tokenService,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match");

        // Check if user exists
        var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {request.Email} already exists");

        // Hash password
        var passwordHash = _passwordService.HashPassword(request.Password);

        // Create user aggregate
        var user = User.Create(request.Email, passwordHash, request.FirstName, request.LastName);

        // Persist
        await _unitOfWork.UserRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Message = "User registered successfully. Please confirm your email."
        };
    }

    /// <summary>
    /// Authenticate user and return tokens
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password");

        // Verify password
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is deactivated");

        // Update last login
        user.UpdateLastLogin();
        await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    /// <summary>
    /// Get user profile
    /// </summary>
    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_profile_{userId}";

        // Try to get from cache
        var cached = await _cacheService.GetAsync<UserProfileDto>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        // Get from database
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found");

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailConfirmed = user.IsEmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        // Cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(30), cancellationToken);

        return profile;
    }

    /// <summary>
    /// Confirm user email
    /// </summary>
    public async Task ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found");

        user.ConfirmEmail();
        await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"user_profile_{userId}", cancellationToken);
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {userId} not found");

        user.Deactivate();
        await _unitOfWork.UserRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"user_profile_{userId}", cancellationToken);
    }
}
