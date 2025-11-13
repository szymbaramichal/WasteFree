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
    string City,
    Pager Pager) : IRequest<ICollection<GarbageOrderDto>>;

public sealed class GetWaitingForAcceptOrdersByCityQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetWaitingForAcceptOrdersByCityQuery, ICollection<GarbageOrderDto>>
{
    public async Task<Result<ICollection<GarbageOrderDto>>> HandleAsync(
        GetWaitingForAcceptOrdersByCityQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.City))
        {
            return PaginatedResult<ICollection<GarbageOrderDto>>.Failure(
                ValidationErrorCodes.GroupCityRequired,
                HttpStatusCode.BadRequest);
        }

        var normalizedCity = request.City.Trim();
        var normalizedCityUpper = normalizedCity.ToUpper();

        var ordersQuery = context.GarbageOrders
            .AsNoTracking()
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageGroup)
            .Include(order => order.GarbageOrderUsers)
                .ThenInclude(orderUser => orderUser.User)
            .Where(order => order.GarbageOrderStatus == GarbageOrderStatus.WaitingForAccept)
            .Where(order => order.GarbageGroup.Address.City != null &&
                            order.GarbageGroup.Address.City.ToUpper() == normalizedCityUpper)
            .OrderBy(order => order.PickupDate)
            .ThenBy(order => order.CreatedDateUtc);

        var totalCount = await ordersQuery.CountAsync(cancellationToken);

        var pagedOrders = await ordersQuery
            .Paginate(request.Pager)
            .ToListAsync(cancellationToken);

        var dtoItems = pagedOrders
            .Select(order => order.MapToGarbageOrderDto())
            .ToList();

        var pager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, totalCount);

        return PaginatedResult<ICollection<GarbageOrderDto>>.PaginatedSuccess(dtoItems, pager);
    }
}
