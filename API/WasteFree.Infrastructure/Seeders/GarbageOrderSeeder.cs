using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class GarbageOrderSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var adminLookup = await context.Users
            .Where(u => u.Username == "garbageadmin1" || u.Username == "garbageadmin2")
            .Select(u => new { u.Username, u.Id })
            .ToDictionaryAsync(u => u.Username, u => u.Id);

        Guid? garbageAdmin1Id = adminLookup.TryGetValue("garbageadmin1", out var id1) ? id1 : null;
        Guid? garbageAdmin2Id = adminLookup.TryGetValue("garbageadmin2", out var id2) ? id2 : null;

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
                Cost = 450m,
                PrepaidUtilizationFeeAmount = 90m
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
                Cost = 180m,
                PrepaidUtilizationFeeAmount = 36m
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
                Cost = 620m,
                PrepaidUtilizationFeeAmount = 124m,
                AssignedGarbageAdminId = garbageAdmin1Id,
                UtilizationFeeAmount = 184m,
                AdditionalUtilizationFeeAmount = 60m,
                UtilizationProofBlobName = "seeded-proof-order-223.jpg",
                UtilizationFeeSubmittedDateUtc = DateTime.UtcNow.AddDays(-1)
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
                Cost = 95m,
                PrepaidUtilizationFeeAmount = 19m
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
                Cost = 310m,
                PrepaidUtilizationFeeAmount = 62m
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333331"),
                GarbageGroupId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                PickupOption = PickupOption.Container,
                ContainerSize = ContainerSize.ContainerSmall,
                DropOffDate = DateTime.UtcNow.Date.AddDays(2),
                PickupDate = DateTime.UtcNow.Date.AddDays(7),
                IsHighPriority = false,
                CollectingService = true,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForAccept,
                Cost = 300m,
                PrepaidUtilizationFeeAmount = 60m
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333332"),
                GarbageGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                PickupOption = PickupOption.Pickup,
                ContainerSize = null,
                DropOffDate = null,
                PickupDate = DateTime.UtcNow.Date.AddDays(6),
                IsHighPriority = true,
                CollectingService = false,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForAccept,
                Cost = 240m,
                PrepaidUtilizationFeeAmount = 48m
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                GarbageGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                PickupOption = PickupOption.SpecialOrder,
                ContainerSize = null,
                DropOffDate = null,
                PickupDate = DateTime.UtcNow.Date.AddDays(12),
                IsHighPriority = true,
                CollectingService = true,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForAccept,
                Cost = 420m,
                PrepaidUtilizationFeeAmount = 84m
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444441"),
                GarbageGroupId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                PickupOption = PickupOption.Pickup,
                ContainerSize = null,
                DropOffDate = null,
                PickupDate = DateTime.UtcNow.Date.AddDays(4),
                IsHighPriority = true,
                CollectingService = true,
                GarbageOrderStatus = GarbageOrderStatus.WaitingForUtilizationFee,
                Cost = 360m,
                PrepaidUtilizationFeeAmount = 72m,
                AssignedGarbageAdminId = garbageAdmin2Id,
                UtilizationFeeAmount = 120m,
                AdditionalUtilizationFeeAmount = 48m,
                UtilizationProofBlobName = "seeded-proof-order-441.jpg",
                UtilizationFeeSubmittedDateUtc = DateTime.UtcNow.AddDays(-2)
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
