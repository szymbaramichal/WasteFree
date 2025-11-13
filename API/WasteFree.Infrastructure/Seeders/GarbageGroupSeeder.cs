using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Models;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Models;

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
                Address = new Address { City = "Kraków", PostalCode = "30-001", Street = "Długa 2" },
                IsPrivate = false
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Warszawa Mokotów ZeroWaste",
                Description = "Neighborhood initiative for composting and waste reduction in Mokotów.",
                Address = new Address { City = "Warszawa", PostalCode = "02-591", Street = "Puławska 145" },
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
