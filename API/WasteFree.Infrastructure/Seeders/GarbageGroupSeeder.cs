using Microsoft.EntityFrameworkCore;
using WasteFree.Shared.Entities;

namespace WasteFree.Infrastructure.Seeders;

public class GarbageGroupSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var seededGroups = new List<GarbageGroup>
        {
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Kraków Centrum Recycling",
                Description = "Group managing recycling pickups for Kraków city center residents.",
                City = "Kraków",
                PostalCode = "30-001",
                Address = "ul. Długa 2",
                IsPrivate = false
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Warszawa Mokotów ZeroWaste",
                Description = "Neighborhood initiative for composting and waste reduction in Mokotów.",
                City = "Warszawa",
                PostalCode = "02-591",
                Address = "ul. Puławska 145",
                IsPrivate = false
            },
            new()
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Gdańsk Eco Harbor",
                Description = "Community hub coordinating shoreline cleanups and recycling efforts.",
                City = "Gdańsk",
                PostalCode = "80-001",
                Address = "ul. Doki 1",
                IsPrivate = false
            }
        };

        var changesMade = false;
        foreach (var group in seededGroups)
        {
            if (!await context.GarbageGroups.AnyAsync(g => g.Id == group.Id))
            {
                await context.GarbageGroups.AddAsync(group);
                changesMade = true;
            }
        }

        if (changesMade)
        {
            await context.SaveChangesAsync();
        }
    }
}
