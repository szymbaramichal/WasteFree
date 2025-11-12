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

public record GetGarbageOrdersQuery(
    Guid GarbageGroupId,
    Guid UserId,
    Pager Pager,
    DateTime? FromDate,
    DateTime? ToDate,
    GarbageOrderStatus[]? Statuses) : IRequest<ICollection<GarbageOrderDto>>;
    
    
public class GetGarbageOrdersQueryHandler(ApplicationDataContext context) 
    : IRequestHandler<GetGarbageOrdersQuery, ICollection<GarbageOrderDto>>
{
    public async Task<Result<ICollection<GarbageOrderDto>>> HandleAsync(GetGarbageOrdersQuery request, CancellationToken cancellationToken)
    {
        var userInGroup = await context.UserGarbageGroups
            .AnyAsync(x => x.GarbageGroupId == request.GarbageGroupId && x.UserId == request.UserId, cancellationToken);
        
        if (!userInGroup)
            return Result<ICollection<GarbageOrderDto>>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.Forbidden);
        
        var ordersQuery = context.GarbageOrders
            .Where(x => x.GarbageGroupId == request.GarbageGroupId)
            .Include(x => x.GarbageGroup)
            .Include(x => x.GarbageOrderUsers)
                .ThenInclude(u => u.User)
            .AsQueryable();
        
        if (request.FromDate.HasValue)
            ordersQuery = ordersQuery.Where(x => x.PickupDate >= request.FromDate.Value);
        
        if (request.ToDate.HasValue)
            ordersQuery = ordersQuery.Where(x => x.PickupDate <= request.ToDate.Value);
        
        if (request.Statuses?.Any() == true)
            ordersQuery = ordersQuery.Where(x => request.Statuses.Contains(x.GarbageOrderStatus));
        
        var totalCount = await ordersQuery.CountAsync(cancellationToken);

        var pagedOrders = await ordersQuery
            .Paginate(request.Pager)
            .ToListAsync(cancellationToken);

        var dtoItems = pagedOrders
            .Select(x => x.MapToGarbageOrderDto())
            .ToList();

        var pager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, totalCount);
        
        return PaginatedResult<ICollection<GarbageOrderDto>>.PaginatedSuccess(dtoItems, pager);
    }
}
