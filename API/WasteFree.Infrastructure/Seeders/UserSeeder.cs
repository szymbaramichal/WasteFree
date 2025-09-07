using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class UserSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Kwakwa5!"));

        var users = new[]
        {
            new User
            {
                Id = Guid.NewGuid(),
                Username = "test1",
                Email = "test1@example.com",
                Description = "Test user 1",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.User,
                LanguagePreference = LanguagePreference.Polish
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "test2",
                Email = "test2@example.com",
                Description = "Test user 2",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.User,
                LanguagePreference = LanguagePreference.Polish
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "test3",
                Email = "test3@example.com",
                Description = "Test user 3",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.User,
                LanguagePreference = LanguagePreference.Polish
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "test4",
                Email = "test4@example.com",
                Description = "Test user 4",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.User,
                LanguagePreference = LanguagePreference.Polish
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "test5",
                Email = "test5@example.com",
                Description = "Test user 5",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.User,
                LanguagePreference = LanguagePreference.Polish
            }
        };

        foreach (var user in users)
        {
            if (!await context.Users.AnyAsync(u => u.Email == user.Email))
            {
                await context.Users.AddAsync(user);
                // Add wallet for the user
                if (!await context.Wallets.AnyAsync(w => w.UserId == user.Id))
                {
                    var wallet = new Wallet
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Funds = 0
                    };
                    await context.Wallets.AddAsync(wallet);
                }
            }
        }
        await context.SaveChangesAsync();
    }
}