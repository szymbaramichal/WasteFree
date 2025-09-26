using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.GarbageGroups.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record GetGarbageGroupDetailsQuery(Guid UserId, Guid GarbageGroupId) : IRequest<GarbageGroupDto>;

public class GetGarbageGroupDetailsQueryHandler(ApplicationDataContext context) 
    : IRequestHandler<GetGarbageGroupDetailsQuery, GarbageGroupDto>
{
    public async Task<Result<GarbageGroupDto>> HandleAsync(GetGarbageGroupDetailsQuery request, 
        CancellationToken cancellationToken)
    {
        var userGroup = await context.UserGarbageGroups
            .Include(x => x.GarbageGroup)
            .ThenInclude(x => x.UserGarbageGroups)
            .Where(x => x.UserId == request.UserId && x.GarbageGroupId == request.GarbageGroupId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if(userGroup is null)
            return Result<GarbageGroupDto>.Failure("NOT_FOUND", HttpStatusCode.NotFound);
        
        return Result<GarbageGroupDto>.Success(userGroup.GarbageGroup.MapToGarbageGroupDto(
            userGroup.GarbageGroup.UserGarbageGroups));
    }
}