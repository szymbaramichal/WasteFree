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
        // Get group with members first
        var group = await context.GarbageGroups
            .Include(g => g.UserGarbageGroups)
            .ThenInclude(ug => ug.User)
            .FirstOrDefaultAsync(g => g.Id == request.GarbageGroupId, cancellationToken);

        if (group is null)
            return Result<GarbageGroupDto>.Failure("NOT_FOUND", HttpStatusCode.NotFound);

        // Check membership/permission
        var isMember = group.UserGarbageGroups.Any(ug => ug.UserId == request.UserId);
        if (!isMember)
            return Result<GarbageGroupDto>.Failure("FORBIDDEN", HttpStatusCode.Forbidden);

        return Result<GarbageGroupDto>.Success(group.MapToGarbageGroupDto(group.UserGarbageGroups));
    }
}