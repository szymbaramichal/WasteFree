using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageGroups.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;

namespace WasteFree.Application.Features.GarbageGroups;

public record GetGarbageGroupDetailsQuery(Guid UserId, Guid GarbageGroupId) : IRequest<GarbageGroupDto>;

public class GetGarbageGroupDetailsQueryHandler(ApplicationDataContext context, IBlobStorageService blobStorageService) 
    : IRequestHandler<GetGarbageGroupDetailsQuery, GarbageGroupDto>
{
    public async Task<Result<GarbageGroupDto>> HandleAsync(GetGarbageGroupDetailsQuery request, 
        CancellationToken cancellationToken)
    {
        // Get group with members first
        var group = await context.GarbageGroups
            .FilterNonPrivate()
            .Include(g => g.UserGarbageGroups)
            .ThenInclude(ug => ug.User)
            .FirstOrDefaultAsync(g => g.Id == request.GarbageGroupId, cancellationToken);

        if (group is null)
            return Result<GarbageGroupDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);

        // Check membership/permission
        var isMember = group.UserGarbageGroups.Any(ug => ug.UserId == request.UserId);
        if (!isMember)
            return Result<GarbageGroupDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);

        var avatarLookup = await group.UserGarbageGroups
            .BuildAvatarUrlLookupAsync(blobStorageService, cancellationToken);

        return Result<GarbageGroupDto>.Success(
            group.MapToGarbageGroupDto(group.UserGarbageGroups, avatarLookup));
    }
}