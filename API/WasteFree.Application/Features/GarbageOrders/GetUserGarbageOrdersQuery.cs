using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;

namespace WasteFree.Application.Features.GarbageOrders;

public record GetUserGarbageOrdersQuery(Guid UserId, Pager Pager) : IRequest<ICollection<GarbageOrderDto>>;

public class GetUserGarbageOrdersQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetUserGarbageOrdersQuery, ICollection<GarbageOrderDto>>
{
    public async Task<Result<ICollection<GarbageOrderDto>>> HandleAsync(
        GetUserGarbageOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var ordersQuery = context.GarbageOrders
            .Where(order => order.GarbageOrderUsers.Any(user => user.UserId == request.UserId))
            .Include(order => order.GarbageGroup)
            .Include(order => order.GarbageOrderUsers)
            .OrderByDescending(order => order.PickupDate);

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
