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
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true
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
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true
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
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true
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
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true
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
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true
            },
            // Garbage administrators
            new User
            {
                Id = Guid.NewGuid(),
                Username = "garbageadmin1",
                Email = "garbageadmin1@example.com",
                Description = "Seeded garbage admin 1",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.GarbageAdmin,
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "garbageadmin2",
                Email = "garbageadmin2@example.com",
                Description = "Seeded garbage admin 2",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.GarbageAdmin,
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true
            }
        };

        foreach (var user in users)
        {
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (existingUser is null)
            {
                existingUser = user;
                await context.Users.AddAsync(existingUser);
            }

            if (!await context.Wallets.AnyAsync(w => w.UserId == existingUser.Id))
            {
                await context.Wallets.AddAsync(new Wallet
                {
                    Id = Guid.CreateVersion7(),
                    UserId = existingUser.Id,
                    Funds = 0
                });
            }

            var hasPrivateGroup = await context.UserGarbageGroups
                .AnyAsync(ugg => ugg.UserId == existingUser.Id && ugg.GarbageGroup.IsPrivate);

            if (hasPrivateGroup)
            {
                continue;
            }

            var privateGroup = new GarbageGroup
            {
                Id = Guid.CreateVersion7(),
                Name = $"{existingUser.Username} Private Group",
                Description = $"Private garbage group for {existingUser.Username}",
                City = existingUser.City ?? string.Empty,
                PostalCode = string.Empty,
                Address = string.Empty,
                IsPrivate = true
            };

            await context.GarbageGroups.AddAsync(privateGroup);
            await context.UserGarbageGroups.AddAsync(new UserGarbageGroup
            {
                Id = Guid.CreateVersion7(),
                UserId = existingUser.Id,
                GarbageGroupId = privateGroup.Id,
                Role = GarbageGroupRole.Owner,
                IsPending = false
            });
        }
        await context.SaveChangesAsync();
    }
}