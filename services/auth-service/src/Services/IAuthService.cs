using AuthService.Models;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string fullName, string email, string password);
        Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password);
    }
}