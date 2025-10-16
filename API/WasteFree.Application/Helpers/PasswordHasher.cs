using System.Security.Cryptography;
using System.Text;

namespace WasteFree.Application.Helpers;

public static class PasswordHasher
{
    public static (byte[] passwordHash, byte[] passwordSalt) GeneratePasswordHashAndSalt(string password)
    {
        using var hmac = new HMACSHA512();

        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    
        return (passwordHash, passwordSalt);
    }

    public static bool IsPasswordValid(string unHashedPassword, byte[] hashedPassword, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);

        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(unHashedPassword));

        return hashedPassword.SequenceEqual(passwordHash);
    }
}