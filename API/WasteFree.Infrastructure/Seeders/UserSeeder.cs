using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Infrastructure.Seeders;

public class UserSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        using var hmac = new HMACSHA512();
        var passwordSalt = hmac.Key;
        var passwordHash = hmac.ComputeHash("Kwakwa5!"u8.ToArray());

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
                IsActive = true,
                Address = new Address { City = "Cracow", PostalCode = "31-019", Street = "Floriańska 1" }
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
                IsActive = true,
                Address = new Address { City = "Cracow", PostalCode = "31-044", Street = "Grodzka 46" }
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
                IsActive = true,
                Address = new Address { City = "Cracow", PostalCode = "31-101", Street = "Straszewskiego 5" }
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
                IsActive = true,
                Address = new Address { City = "Warsaw", PostalCode = "00-029", Street = "Nowy Świat 15" }
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
                IsActive = true,
                Address = new Address { City = "Warsaw", PostalCode = "00-325", Street = "Krakowskie Przedmieście 20" }
            },
            // System administrators
            new User
            {
                Id = Guid.NewGuid(),
                Username = "admin1",
                Email = "admin1@wastefreecloud.pl",
                Description = "Seeded system admin 1",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.Admin,
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true,
                Address = new Address { City = "Cracow", PostalCode = "31-042", Street = "Rynek Główny 3" }
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "admin2",
                Email = "admin2@wastefreecloud.pl",
                Description = "Seeded system admin 2",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                Role = UserRole.Admin,
                LanguagePreference = LanguagePreference.Polish,
                IsActive = true,
                Address = new Address { City = "Cracow", PostalCode = "31-007", Street = "Szewska 12" }
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
                IsActive = true,
                ConsentsAgreed = false,
                Address = new Address { City = "Cracow", PostalCode = "31-872", Street = "Szewska 7" }
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
                IsActive = true,
                ConsentsAgreed = false,
                Address = new Address { City = "Warsaw", PostalCode = "02-715", Street = "Puławska 145" }
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

            var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.UserId == existingUser.Id);
            var initialFunds = existingUser.Role switch
            {
                UserRole.User => 500d,
                UserRole.GarbageAdmin => 200d,
                _ => 0d
            };

            if (wallet is null)
            {
                wallet = new Wallet
                {
                    Id = Guid.CreateVersion7(),
                    UserId = existingUser.Id,
                    Funds = initialFunds
                };
                await context.Wallets.AddAsync(wallet);
            }
            else
            {
                wallet.Funds = initialFunds;
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
                Address = existingUser.Address,
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