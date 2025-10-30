using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageGroupOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageGroupOrders;

public record GetGarbageGroupOrdersQuery(
    Guid GarbageGroupId,
    Guid UserId,
    DateTime? FromDate,
    DateTime? ToDate,
    GarbageOrderStatus[]? Statuses) : IRequest<ICollection<GarbageGroupOrderDto>>;
    
    
public class GetGarbageGroupOrdersQueryHandler(ApplicationDataContext context) 
    : IRequestHandler<GetGarbageGroupOrdersQuery, ICollection<GarbageGroupOrderDto>>
{
    public async Task<Result<ICollection<GarbageGroupOrderDto>>> HandleAsync(GetGarbageGroupOrdersQuery request, CancellationToken cancellationToken)
    {
        var userInGroup = await context.UserGarbageGroups
            .AnyAsync(x => x.GarbageGroupId == request.GarbageGroupId && x.UserId == request.UserId, cancellationToken);
        
        if (!userInGroup)
            return Result<ICollection<GarbageGroupOrderDto>>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.Forbidden);
        
        var ordersQuery = context.GarbageOrders
            .Where(x => x.GarbageGroupId == request.GarbageGroupId)
            .Include(x => x.GarbageOrderUsers)
            .AsQueryable();
        
        if (request.FromDate.HasValue)
            ordersQuery = ordersQuery.Where(x => x.PickupDate >= request.FromDate.Value);
        
        if (request.ToDate.HasValue)
            ordersQuery = ordersQuery.Where(x => x.PickupDate <= request.ToDate.Value);
        
        if (request.Statuses?.Any() == true)
            ordersQuery = ordersQuery.Where(x => request.Statuses.Contains(x.GarbageOrderStatus));
        
        var orders = await ordersQuery
            .Select(x => x.MapToGarbageGroupOrderDto())
            .ToListAsync(cancellationToken);
        
        return Result<ICollection<GarbageGroupOrderDto>>.Success(orders);
    }
}