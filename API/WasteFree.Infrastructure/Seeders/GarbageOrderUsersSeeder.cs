using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Entities;

namespace WasteFree.Infrastructure.Seeders;

public class GarbageOrderUsersSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var participants = new List<(Guid OrderId, string Username, bool HasAcceptedPayment, decimal ShareAmount)>
        {
            (Guid.Parse("22222222-2222-2222-2222-222222222221"), "test1", true, 225m),
            (Guid.Parse("22222222-2222-2222-2222-222222222221"), "test5", true, 225m),

            (Guid.Parse("22222222-2222-2222-2222-222222222222"), "test2", false, 90m),
            (Guid.Parse("22222222-2222-2222-2222-222222222222"), "test5", false, 90m),

            (Guid.Parse("22222222-2222-2222-2222-222222222223"), "test1", true, 310m),
            (Guid.Parse("22222222-2222-2222-2222-222222222223"), "test2", true, 310m),

            (Guid.Parse("22222222-2222-2222-2222-222222222224"), "test4", true, 95m),

            (Guid.Parse("22222222-2222-2222-2222-222222222225"), "test3", true, 155m),
            (Guid.Parse("22222222-2222-2222-2222-222222222225"), "test5", true, 155m)
        };

        var orderIds = participants.Select(p => p.OrderId).Distinct().ToArray();
        var existingOrderIds = await context.GarbageOrders
            .Where(o => orderIds.Contains(o.Id))
            .Select(o => o.Id)
            .ToListAsync();

        var usernames = participants.Select(p => p.Username).Distinct().ToArray();
        var users = await context.Users
            .Where(u => usernames.Contains(u.Username))
            .Select(u => new { u.Id, u.Username })
            .ToListAsync();
        var userLookup = users.ToDictionary(u => u.Username, u => u.Id);

        var changesMade = false;
        foreach (var participant in participants)
        {
            if (!existingOrderIds.Contains(participant.OrderId))
            {
                continue;
            }

            if (!userLookup.TryGetValue(participant.Username, out var userId))
            {
                continue;
            }

            var alreadyExists = await context.GarbageOrderUsers
                .AnyAsync(gou => gou.GarbageOrderId == participant.OrderId && gou.UserId == userId);

            if (alreadyExists)
            {
                continue;
            }

            await context.GarbageOrderUsers.AddAsync(new GarbageOrderUsers
            {
                Id = Guid.CreateVersion7(),
                GarbageOrderId = participant.OrderId,
                UserId = userId,
                HasAcceptedPayment = participant.HasAcceptedPayment,
                ShareAmount = participant.ShareAmount
            });
            changesMade = true;
        }

        if (changesMade)
        {
            await context.SaveChangesAsync();
        }
    }
}
