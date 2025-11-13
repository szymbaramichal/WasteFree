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

public sealed record GetWaitingForAcceptOrdersByCityQuery(
    Guid GarbageAdminId,
    Pager Pager) : IRequest<ICollection<GarbageOrderDto>>;

public sealed class GetWaitingForAcceptOrdersByCityQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetWaitingForAcceptOrdersByCityQuery, ICollection<GarbageOrderDto>>
{
    public async Task<Result<ICollection<GarbageOrderDto>>> HandleAsync(
        GetWaitingForAcceptOrdersByCityQuery request,
        CancellationToken cancellationToken)
    {
        var adminAddress = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == request.GarbageAdminId)
            .Select(user => new
            {
                user.Id,
                user.Address.City,
                user.Address.Latitude,
                user.Address.Longitude
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (adminAddress is null)
        {
            return PaginatedResult<ICollection<GarbageOrderDto>>.Failure(
                ApiErrorCodes.InvalidUser,
                HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(adminAddress.City))
        {
            return PaginatedResult<ICollection<GarbageOrderDto>>.Failure(
                ValidationErrorCodes.GroupCityRequired,
                HttpStatusCode.BadRequest);
        }

        var normalizedCity = adminAddress.City.Trim();
        var normalizedCityUpper = normalizedCity.ToUpper();

        var ordersQuery = context.GarbageOrders
            .AsNoTracking()
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageGroup)
            .Include(order => order.GarbageOrderUsers)
                .ThenInclude(orderUser => orderUser.User)
            .Where(order => order.GarbageOrderStatus == GarbageOrderStatus.WaitingForAccept)
            .Where(order => order.GarbageGroup.Address.City != null &&
                            order.GarbageGroup.Address.City.ToUpper() == normalizedCityUpper);

        var referencePoint = (Latitude: adminAddress.Latitude, Longitude: adminAddress.Longitude);

        var totalCount = await ordersQuery.CountAsync(cancellationToken);

        var orderedQuery = ordersQuery
            .Select(order => new
            {
                Order = order,
                Distance = referencePoint.Latitude.HasValue && referencePoint.Longitude.HasValue &&
                           order.GarbageGroup.Address.Latitude.HasValue &&
                           order.GarbageGroup.Address.Longitude.HasValue
                    ? GarbageOrderDistanceHelpers.HaversineDistance(
                        referencePoint.Latitude.Value,
                        referencePoint.Longitude.Value,
                        order.GarbageGroup.Address.Latitude.Value,
                        order.GarbageGroup.Address.Longitude.Value)
                    : (double?)null
            })
            .OrderBy(x => x.Distance ?? double.MaxValue)
            .ThenBy(x => x.Order.PickupDate)
            .ThenBy(x => x.Order.CreatedDateUtc);

        var pagedOrders = await orderedQuery
            .Paginate(request.Pager)
            .ToListAsync(cancellationToken);

        var dtoItems = pagedOrders
            .Select(entry => entry.Order.MapToGarbageOrderDto(distanceInKilometers: entry.Distance))
            .ToList();

        var pager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, totalCount);

        return PaginatedResult<ICollection<GarbageOrderDto>>.PaginatedSuccess(dtoItems, pager);
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

        return EarthRadiusKm * c;
    }
}
