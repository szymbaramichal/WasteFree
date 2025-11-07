using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public record GetUserGarbageOrdersQuery(Guid UserId) : IRequest<ICollection<GarbageOrderDto>>;

public class GetUserGarbageOrdersQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetUserGarbageOrdersQuery, ICollection<GarbageOrderDto>>
{
    public async Task<Result<ICollection<GarbageOrderDto>>> HandleAsync(
        GetUserGarbageOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await context.GarbageOrders
            .Where(order => order.GarbageOrderUsers.Any(user => user.UserId == request.UserId))
            .Include(order => order.GarbageOrderUsers)
            .OrderByDescending(order => order.PickupDate)
            .Select(order => order.MapToGarbageOrderDto())
            .ToListAsync(cancellationToken);

        return Result<ICollection<GarbageOrderDto>>.Success(orders);
    }
}
