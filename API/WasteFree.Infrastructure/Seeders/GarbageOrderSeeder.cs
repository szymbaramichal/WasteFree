using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class GarbageOrderSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var seededOrders = new List<GarbageOrder>
        {
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222221"),
                GarbageGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                PickupOption = PickupOption.Container,
                ContainerSize = ContainerSize.ContainerMedium,
                DropOffDate = DateTime.UtcNow.Date.AddDays(1),
                PickupDate = DateTime.UtcNow.Date.AddDays(5),
                IsHighPriority = true,
                CollectingService = true,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForPickup,
                Cost = 450m
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                GarbageGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                PickupOption = PickupOption.Pickup,
                ContainerSize = null,
                DropOffDate = null,
                PickupDate = DateTime.UtcNow.Date.AddDays(3),
                IsHighPriority = false,
                CollectingService = true,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForPayment,
                Cost = 180m
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222223"),
                GarbageGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                PickupOption = PickupOption.SpecialOrder,
                ContainerSize = null,
                DropOffDate = null,
                PickupDate = DateTime.UtcNow.Date.AddDays(10),
                IsHighPriority = true,
                CollectingService = true,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForUtilizationFee,
                Cost = 620m
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222224"),
                GarbageGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                PickupOption = PickupOption.SmallPickup,
                ContainerSize = null,
                DropOffDate = null,
                PickupDate = DateTime.UtcNow.Date.AddDays(-2),
                IsHighPriority = false,
                CollectingService = false,
                GarbageOrderStatus = GarbageOrderStatus.Completed,
                Cost = 95m
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222225"),
                GarbageGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                PickupOption = PickupOption.Pickup,
                ContainerSize = null,
                DropOffDate = null,
                PickupDate = DateTime.UtcNow.Date.AddDays(15),
                IsHighPriority = false,
                CollectingService = true,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForAccept,
                Cost = 310m
            }
        };

        var changesMade = false;
        foreach (var order in seededOrders)
        {
            if (!await context.GarbageOrders.AnyAsync(o => o.Id == order.Id))
            {
                await context.GarbageOrders.AddAsync(order);
                changesMade = true;
            }
        }

        if (changesMade)
        {
            await context.SaveChangesAsync();
        }
    }
}
