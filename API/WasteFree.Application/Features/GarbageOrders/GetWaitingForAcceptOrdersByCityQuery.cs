using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record GetWaitingForAcceptOrdersByCityQuery(
    Guid GarbageAdminId,
    Pager Pager) : IRequest<ICollection<GarbageOrderSummaryDto>>;

public sealed class GetWaitingForAcceptOrdersByCityQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetWaitingForAcceptOrdersByCityQuery, ICollection<GarbageOrderSummaryDto>>
{
    public async Task<Result<ICollection<GarbageOrderSummaryDto>>> HandleAsync(
        GetWaitingForAcceptOrdersByCityQuery request,
        CancellationToken cancellationToken)
    {
        var admin = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == request.GarbageAdminId, cancellationToken);

        if (admin is null)
        {
            return PaginatedResult<ICollection<GarbageOrderSummaryDto>>.Failure(
                ApiErrorCodes.InvalidUser,
                HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(admin.Address.City))
        {
            return PaginatedResult<ICollection<GarbageOrderSummaryDto>>.Failure(
                ValidationErrorCodes.GroupCityRequired,
                HttpStatusCode.BadRequest);
        }

        var adminPickupOptions = admin.PickupOptionsList ?? [];
        if (adminPickupOptions.Length == 0)
        {
            var emptyPager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, 0);
            return PaginatedResult<ICollection<GarbageOrderSummaryDto>>.PaginatedSuccess([], emptyPager);
        }

        var referencePoint = (Latitude: admin.Address.Latitude, Longitude: admin.Address.Longitude);
        var normalizedCity = admin.Address.City.ToLower();
        var pickupOptionsFilter = adminPickupOptions;

        var ordersQuery = context.GarbageOrders
            .AsNoTracking()
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageGroup)
            .Where(order => order.GarbageOrderStatus == GarbageOrderStatus.WaitingForAccept
                            && order.GarbageGroup.Address.City.ToLower() == normalizedCity
                            && pickupOptionsFilter.Contains(order.PickupOption));
        
        var totalCount = await ordersQuery.CountAsync(cancellationToken);

        var orders = await ordersQuery.ToListAsync(cancellationToken);

        // Distance ordering is evaluated client-side because the Haversine helper cannot be translated by EF Core.
        var ordersWithDistance = orders
            .Select(order =>
            {
                double? distance = null;
                var groupAddress = order.GarbageGroup?.Address;

                if (referencePoint.Latitude.HasValue && referencePoint.Longitude.HasValue &&
                    groupAddress?.Latitude.HasValue == true &&
                    groupAddress.Longitude.HasValue)
                {
                    distance = GarbageOrderDistanceHelpers.HaversineDistance(
                        referencePoint.Latitude.Value,
                        referencePoint.Longitude.Value,
                        groupAddress.Latitude.Value,
                        groupAddress.Longitude.Value);
                }

                return (Order: order, Distance: distance);
            })
            .ToList();

        var orderedOrders = ordersWithDistance
            .OrderBy(entry => entry.Distance ?? double.MaxValue)
            .ThenBy(entry => entry.Order.PickupDate)
            .ThenBy(entry => entry.Order.CreatedDateUtc)
            .ToList();

        var skip = (request.Pager.PageNumber - 1) * request.Pager.PageSize;
        if (skip < 0)
        {
            skip = 0;
        }

        var pagedOrders = orderedOrders
            .Skip(skip)
            .Take(request.Pager.PageSize)
            .ToList();

        var dtoItems = pagedOrders
            .Select(entry => entry.Order.MapToGarbageOrderSummaryDto(distanceInKilometers: entry.Distance))
            .ToList();

        var pager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, totalCount);

        return PaginatedResult<ICollection<GarbageOrderSummaryDto>>.PaginatedSuccess(dtoItems, pager);
    }
}

public static partial class GarbageOrderDistanceHelpers
{
    private const double EarthRadiusKm = 6371.0;

    private static double ToRadians(double angle) => Math.PI * angle / 180.0;

    public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
            var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Pow(Math.Sin(dLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        c = c * 1.25;
        return EarthRadiusKm * c;
    }
}
