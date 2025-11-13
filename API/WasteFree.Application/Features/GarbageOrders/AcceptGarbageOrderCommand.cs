using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record AcceptGarbageOrderCommand(Guid GarbageOrderId, Guid GarbageAdminId) : IRequest<GarbageOrderDto>;

public sealed class AcceptGarbageOrderCommandHandler(ApplicationDataContext context)
    : IRequestHandler<AcceptGarbageOrderCommand, GarbageOrderDto>
{
    public async Task<Result<GarbageOrderDto>> HandleAsync(
        AcceptGarbageOrderCommand request,
        CancellationToken cancellationToken)
    {
        var garbageOrder = await context.GarbageOrders
            .Include(order => order.GarbageGroup)
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageOrderUsers)
                .ThenInclude(orderUser => orderUser.User)
            .FirstOrDefaultAsync(order => order.Id == request.GarbageOrderId, cancellationToken);

        if (garbageOrder is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        if (garbageOrder.GarbageOrderStatus != GarbageOrderStatus.WaitingForAccept)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var garbageAdminExists = await context.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == request.GarbageAdminId, cancellationToken);

        if (!garbageAdminExists)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.InvalidUser, HttpStatusCode.BadRequest);
        }

        garbageOrder.GarbageOrderStatus = GarbageOrderStatus.WaitingForPickup;
        garbageOrder.AssignedGarbageAdminId = request.GarbageAdminId;

        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(garbageOrder)
            .Reference(order => order.AssignedGarbageAdmin)
            .LoadAsync(cancellationToken);

        return Result<GarbageOrderDto>.Success(garbageOrder.MapToGarbageOrderDto());
    }
}
