using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteFree.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Password: Kwakwa5!
            using var hmac = new HMACSHA512();

            var passwordSalt = hmac.Key;
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Kwakwa5!"));
            var users = new[]
            {
                new {
                    Id = Guid.NewGuid(),
                    Username = "test1",
                    Email = "test1@example.com",
                    Description = "Test user 1",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedDateUtc = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                },
                new {
                    Id = Guid.NewGuid(),
                    Username = "test2",
                    Email = "test2@example.com",
                    Description = "Test user 2",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedDateUtc = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                },
                new {
                    Id = Guid.NewGuid(),
                    Username = "test3",
                    Email = "test3@example.com",
                    Description = "Test user 3",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedDateUtc = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                },
                new {
                    Id = Guid.NewGuid(),
                    Username = "test4",
                    Email = "test4@example.com",
                    Description = "Test user 4",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedDateUtc = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                },
                new {
                    Id = Guid.NewGuid(),
                    Username = "test5",
                    Email = "test5@example.com",
                    Description = "Test user 5",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedDateUtc = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                }
            };

            foreach (var user in users)
            {
                migrationBuilder.InsertData(
                    table: "Users",
                    columns: new[] { "Id", "Username", "Email", "Description", "PasswordHash", "PasswordSalt", "CreatedDateUtc", "CreatedBy" },
                    values: new object[] { user.Id, user.Username, user.Email, user.Description, user.PasswordHash, user.PasswordSalt, user.CreatedDateUtc, user.CreatedBy }
                );
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (int i = 1; i <= 5; i++)
            {
                migrationBuilder.DeleteData(
                    table: "Users",
                    keyColumn: "Id",
                    keyValue: i
                );
            }
        }
    }
}
