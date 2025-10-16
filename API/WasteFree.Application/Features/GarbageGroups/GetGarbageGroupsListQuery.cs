using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record GetGarbageGroupsListQuery(Guid UserId) : IRequest<ICollection<GarbageGroupInfoDto>>;

public class GetWalletBalanceQueryHandler(ApplicationDataContext context) 
    : IRequestHandler<GetGarbageGroupsListQuery, ICollection<GarbageGroupInfoDto>>
{
    public async Task<Result<ICollection<GarbageGroupInfoDto>>> HandleAsync(GetGarbageGroupsListQuery request, 
        CancellationToken cancellationToken)
    {
        var userGroups = await context.UserGarbageGroups
            .FilterNonPrivate()
            .Include(x => x.GarbageGroup)
            .Where(x => x.UserId == request.UserId && !x.IsPending)
            .Select(x => new GarbageGroupInfoDto
            { 
                Id = x.GarbageGroupId,
                Name = x.GarbageGroup.Name,
                IsUserOwner = x.Role == GarbageGroupRole.Owner,
                City = x.GarbageGroup.City,
                PostalCode = x.GarbageGroup.PostalCode,
                Address = x.GarbageGroup.Address
            })
            .ToListAsync(cancellationToken);

        return Result<ICollection<GarbageGroupInfoDto>>.Success(userGroups);
    }
}