using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Application.Services.GarbageOrders;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public record CalculateGarbageOrderCostQuery(
    Guid GarbageGroupId,
    Guid CurrentUserId,
    PickupOption PickupOption,
    ContainerSize? ContainerSize,
    DateTime? DropOffDate,
    DateTime PickupDate,
    bool IsHighPriority,
    bool CollectingService) : IRequest<GarbageOrderCostDto>;

public class CalculateGarbageOrderCostQueryHandler(
    ApplicationDataContext context,
    IGarbageOrderCostCalculator costCalculator)
    : IRequestHandler<CalculateGarbageOrderCostQuery, GarbageOrderCostDto>
{
    public async Task<Result<GarbageOrderCostDto>> HandleAsync(
        CalculateGarbageOrderCostQuery request,
        CancellationToken cancellationToken)
    {
        var isOwner = await context.UserGarbageGroups
            .AsNoTracking()
            .AnyAsync(
                x => x.GarbageGroupId == request.GarbageGroupId
                     && x.UserId == request.CurrentUserId
                     && x.Role == GarbageGroupRole.Owner,
                cancellationToken);

        if (!isOwner)
        {
            return Result<GarbageOrderCostDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);
        }

        var estimatedCost = costCalculator.CalculateEstimate(
            request.PickupOption,
            request.ContainerSize,
            request.DropOffDate,
            request.PickupDate,
            request.IsHighPriority,
            request.CollectingService);

        return Result<GarbageOrderCostDto>.Success(new GarbageOrderCostDto
        {
            EstimatedCost = estimatedCost
        });
    }
}
