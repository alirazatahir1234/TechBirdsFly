using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.ExternalServices;

/// <summary>
/// Password hashing and verification using PBKDF2
/// </summary>
public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, _algorithm);
            byte[] hash = pbkdf2.GetBytes(KeySize);

            byte[] hashWithSalt = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, hashWithSalt, 0, SaltSize);
            Array.Copy(hash, 0, hashWithSalt, SaltSize, KeySize);

            return Convert.ToBase64String(hashWithSalt);
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            byte[] hashWithSalt = Convert.FromBase64String(hash);
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, _algorithm);
            byte[] hashOfInput = pbkdf2.GetBytes(KeySize);

            for (int i = 0; i < KeySize; i++)
            {
                if (hashWithSalt[i + SaltSize] != hashOfInput[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
