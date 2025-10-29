using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageGroupOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageGroupOrders;

public record GarbageGroupOrderCommand (
    Guid GarbageGroupId,
    Guid CurrentUserId,
    PickupOption PickupOption,
    ICollection<Guid> UserIds,
    ContainerSize? ContainerSize,
    DateTime? DropOffDate,
    DateTime PickupDate,
    bool IsHighPriority,
    bool CollectingService) : IRequest<GarbageGroupOrderDto>;

public class GarbageGroupOrderCommandHandler(ApplicationDataContext context) 
    : IRequestHandler<GarbageGroupOrderCommand, GarbageGroupOrderDto>
{
    public async Task<Result<GarbageGroupOrderDto>> HandleAsync(GarbageGroupOrderCommand request, CancellationToken cancellationToken)
    {
        var userGroup = await context.UserGarbageGroups
            .FirstOrDefaultAsync(x => x.GarbageGroupId == request.GarbageGroupId
                && x.UserId == request.CurrentUserId && x.Role == GarbageGroupRole.Owner, cancellationToken);
        
        if (userGroup is null)
            return Result<GarbageGroupOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);
        
        var groupUsers = await context.UserGarbageGroups
            .Where(x => x.GarbageGroupId == request.GarbageGroupId)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);
        
        if (!request.UserIds.All(id => groupUsers.Contains(id)))
            return Result<GarbageGroupOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);
        
        var garbageOrder = new GarbageOrder
        {
            Id = Guid.CreateVersion7(),
            PickupOption = request.PickupOption,
            ContainerSize = request.ContainerSize,
            DropOffDate = request.DropOffDate,
            PickupDate = request.PickupDate,
            IsHighPriority = request.IsHighPriority,
            CollectingService = request.CollectingService,
            GarbageOrderStatus = GarbageOrderStatus.Created,
            GarbageGroupId = request.GarbageGroupId
        };
        
        foreach (var userId in request.UserIds)
        {
            garbageOrder.GarbageOrderUsers.Add(new GarbageOrderUsers
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                GarbageOrderId = garbageOrder.Id,
            });
        }
        
        context.Add(garbageOrder);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result<GarbageGroupOrderDto>.Success(garbageOrder.MapToGarbageGroupOrderDto());
    }
}