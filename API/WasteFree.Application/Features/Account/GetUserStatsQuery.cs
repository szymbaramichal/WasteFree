using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Account.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.Account;

public record GetUserStatsQuery(Guid UserId) : IRequest<UserStatsDto>;

public class GetUserStatsQueryHandler(ApplicationDataContext context) : IRequestHandler<GetUserStatsQuery, UserStatsDto>
{
    public async Task<Result<UserStatsDto>> HandleAsync(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var completedOrders = await context.GarbageOrders
            .AsNoTracking()
            .Where(o => o.GarbageOrderUsers.Any(u => u.UserId == request.UserId) &&
                        o.GarbageOrderStatus == GarbageOrderStatus.Completed)
            .Select(o => new { o.Cost, o.PickupOption })
            .ToListAsync(cancellationToken);

        var communityCount = await context.UserGarbageGroups
            .CountAsync(ugg => ugg.UserId == request.UserId, cancellationToken);

        var totalCost = completedOrders.Sum(o => o.Cost);
        var savings = totalCost * 0.2m; // 20% savings

        var wasteReduced = completedOrders.Sum(o => GetWeight(o.PickupOption));
        var collections = completedOrders.Count;

        return Result<UserStatsDto>.Success(new UserStatsDto(savings, wasteReduced, collections, communityCount));
    }

    private static double GetWeight(PickupOption option)
    {
        return option switch
        {
            PickupOption.SmallPickup => 5,
            PickupOption.Pickup => 20,
            PickupOption.Container => 100,
            PickupOption.SpecialOrder => 50,
            _ => 0
        };
    }
}
