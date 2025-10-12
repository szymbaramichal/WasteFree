using Microsoft.EntityFrameworkCore;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class UserGarbageGroupSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var memberships = new[]
        {
            new { Username = "test1", GroupId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Role = GarbageGroupRole.Owner, IsPending = false },
            new { Username = "test2", GroupId = Guid.Parse("66666666-6666-6666-6666-666666666666"), Role = GarbageGroupRole.Owner, IsPending = false },
            new { Username = "test3", GroupId = Guid.Parse("77777777-7777-7777-7777-777777777777"), Role = GarbageGroupRole.User, IsPending = true },
            new { Username = "test4", GroupId = Guid.Parse("77777777-7777-7777-7777-777777777777"), Role = GarbageGroupRole.Owner, IsPending = false },
            new { Username = "test5", GroupId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Role = GarbageGroupRole.User, IsPending = false }
        };

        var usernames = memberships.Select(m => m.Username).Distinct().ToArray();
        var users = await context.Users
            .Where(u => usernames.Contains(u.Username))
            .ToDictionaryAsync(u => u.Username, u => u.Id);

        var groupIds = memberships.Select(m => m.GroupId).Distinct().ToArray();
        var existingGroups = await context.GarbageGroups
            .Where(g => groupIds.Contains(g.Id))
            .Select(g => g.Id)
            .ToListAsync();

        var changesMade = false;
        foreach (var membership in memberships)
        {
            if (!users.TryGetValue(membership.Username, out var userId))
            {
                continue;
            }

            if (!existingGroups.Contains(membership.GroupId))
            {
                continue;
            }

            var alreadyExists = await context.UserGarbageGroups.AnyAsync(ugg =>
                ugg.UserId == userId && ugg.GarbageGroupId == membership.GroupId);

            if (alreadyExists)
            {
                continue;
            }

            await context.UserGarbageGroups.AddAsync(new UserGarbageGroup
            {
                UserId = userId,
                GarbageGroupId = membership.GroupId,
                Role = membership.Role,
                IsPending = membership.IsPending
            });
            changesMade = true;
        }

        if (changesMade)
        {
            await context.SaveChangesAsync();
        }
    }
}
