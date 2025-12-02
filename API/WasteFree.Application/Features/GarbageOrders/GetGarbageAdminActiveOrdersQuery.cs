using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record GetGarbageAdminActiveOrdersQuery(Guid GarbageAdminId, Pager Pager) : IRequest<ICollection<GarbageOrderDto>>;

public sealed class GetGarbageAdminActiveOrdersQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetGarbageAdminActiveOrdersQuery, ICollection<GarbageOrderDto>>
{
    public async Task<Result<ICollection<GarbageOrderDto>>> HandleAsync(
        GetGarbageAdminActiveOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var garbageAdmin = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == request.GarbageAdminId, cancellationToken);

        if (garbageAdmin is null)
        {
            return Result<ICollection<GarbageOrderDto>>.Failure(ApiErrorCodes.InvalidUser, HttpStatusCode.BadRequest);
        }

        var ordersQuery = context.GarbageOrders
            .AsNoTracking()
            .Include(order => order.GarbageGroup)
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageOrderUsers)
                .ThenInclude(orderUser => orderUser.User)
            .Where(order => order.AssignedGarbageAdminId == request.GarbageAdminId)
            .Where(order => 
                order.GarbageOrderStatus == GarbageOrderStatus.WaitingForUtilizationFee
                || order.GarbageOrderStatus == GarbageOrderStatus.WaitingForPickup)
            .OrderBy(order => order.PickupDate)
            .ThenBy(order => order.CreatedDateUtc);

        var totalCount = await ordersQuery.CountAsync(cancellationToken);

        var pagedOrders = await ordersQuery
            .Paginate(request.Pager)
            .ToListAsync(cancellationToken);

        var adminLatitude = garbageAdmin.Address?.Latitude;
        var adminLongitude = garbageAdmin.Address?.Longitude;

        var dtoItems = pagedOrders
            .Select(order =>
            {
                double? distance = null;
                var groupAddress = order.GarbageGroup?.Address;

                if (adminLatitude.HasValue && adminLongitude.HasValue &&
                    groupAddress?.Latitude.HasValue == true &&
                    groupAddress.Longitude.HasValue)
                {
                    distance = GarbageOrderDistanceHelpers.HaversineDistance(
                        adminLatitude.Value,
                        adminLongitude.Value,
                        groupAddress.Latitude.Value,
                        groupAddress.Longitude.Value);
                }

                return order.MapToGarbageOrderDto(distance);
            })
            .ToList();

        var pager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, totalCount);

        return PaginatedResult<ICollection<GarbageOrderDto>>.PaginatedSuccess(dtoItems, pager);
    }
}
