using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;
using WasteFree.Domain.Interfaces;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record GetAssignedGarbageAdminAvatarUrlQuery(Guid OrderId, Guid RequesterId) : IRequest<GarbageOrderDetailsDto>;

public sealed class GetAssignedGarbageAdminAvatarUrlQueryHandler(
    ApplicationDataContext context,
    IBlobStorageService blobStorageService) : IRequestHandler<GetAssignedGarbageAdminAvatarUrlQuery, GarbageOrderDetailsDto>
{
    public async Task<Result<GarbageOrderDetailsDto>> HandleAsync(
        GetAssignedGarbageAdminAvatarUrlQuery request,
        CancellationToken cancellationToken)
    {
        var order = await context.GarbageOrders
            .AsNoTracking()
            .Include(x => x.AssignedGarbageAdmin)
            .Include(x => x.GarbageOrderUsers)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result<GarbageOrderDetailsDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        var isRequesterAssignedAdmin = order.AssignedGarbageAdminId == request.RequesterId;

        var isParticipant = isRequesterAssignedAdmin || order.GarbageOrderUsers
            .Any(x => x.UserId == request.RequesterId);

        if (!isParticipant)
        {
            var isGroupMember = await context.UserGarbageGroups
                .AsNoTracking()
                .AnyAsync(x => x.GarbageGroupId == order.GarbageGroupId && x.UserId == request.RequesterId, cancellationToken);

            if (!isGroupMember)
            {
                return Result<GarbageOrderDetailsDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
            }
        }

        string? avatarUrl = null;

        var avatarName = order.AssignedGarbageAdmin?.AvatarName;
        if (!string.IsNullOrWhiteSpace(avatarName))
        {
            avatarUrl = await blobStorageService.GetReadSasUrlAsync(
                BlobContainerNames.Avatars,
                avatarName,
                TimeSpan.FromMinutes(5),
                cancellationToken);
        }

        var userAvatarsUrls = new Dictionary<Guid, string>();

        var participantsWithAvatars = order.GarbageOrderUsers
            .Where(x => x.User is not null && !string.IsNullOrWhiteSpace(x.User.AvatarName))
            .Select(x => new { x.UserId, x.User.AvatarName })
            .Distinct()
            .ToArray();

        if (participantsWithAvatars.Length > 0)
        {
            var avatarTasks = participantsWithAvatars
                .Select(async participant => new
                {
                    participant.UserId,
                    Url = participant.AvatarName is not null ?
                        await blobStorageService.GetReadSasUrlAsync(
                        BlobContainerNames.Avatars,
                        participant.AvatarName,
                        TimeSpan.FromMinutes(5),
                        cancellationToken)
                        : ""
                });

            var resolvedAvatars = await Task.WhenAll(avatarTasks);

            foreach (var resolvedAvatar in resolvedAvatars)
            {
                if (!string.IsNullOrWhiteSpace(resolvedAvatar.Url))
                {
                    userAvatarsUrls[resolvedAvatar.UserId] = resolvedAvatar.Url!;
                }
            }
        }

        return Result<GarbageOrderDetailsDto>.Success(new GarbageOrderDetailsDto(avatarUrl, userAvatarsUrls));
    }
}
