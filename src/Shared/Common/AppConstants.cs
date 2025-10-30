namespace TechBirdsFly.Shared.Common;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    public const string DefaultCacheKeyPrefix = "tbf_";
    public const int DefaultCacheDurationMinutes = 30;
    public const int JwtExpirationMinutes = 60;
    public const string CorrelationIdHeader = "X-Correlation-Id";
    public const string RequestIdHeader = "X-Request-Id";

    // Service names
    public static class ServiceNames
    {
        public const string AuthService = "AuthService";
        public const string BillingService = "BillingService";
        public const string GeneratorService = "GeneratorService";
        public const string AdminService = "AdminService";
        public const string ImageService = "ImageService";
        public const string UserService = "UserService";
    }

    // Environment names
    public static class Environments
    {
        public const string Development = "Development";
        public const string Staging = "Staging";
        public const string Production = "Production";
    }

    // Cache keys
    public static class CacheKeys
    {
        public static string User(Guid userId) => $"{DefaultCacheKeyPrefix}user_{userId}";
        public static string UserEmail(string email) => $"{DefaultCacheKeyPrefix}user_email_{email}";
        public static string Token(Guid userId) => $"{DefaultCacheKeyPrefix}token_{userId}";
        public static string Admin(Guid adminId) => $"{DefaultCacheKeyPrefix}admin_{adminId}";
        public static string Settings(string key) => $"{DefaultCacheKeyPrefix}settings_{key}";
    }
}
