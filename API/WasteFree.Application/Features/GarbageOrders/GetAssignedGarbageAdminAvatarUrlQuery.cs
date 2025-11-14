using System;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;
using WasteFree.Domain.Interfaces;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record GetAssignedGarbageAdminAvatarUrlQuery(Guid OrderId, Guid RequesterId) : IRequest<GarbageAdminAvatarUrlDto>;

public sealed class GetAssignedGarbageAdminAvatarUrlQueryHandler(
    ApplicationDataContext context,
    IBlobStorageService blobStorageService) : IRequestHandler<GetAssignedGarbageAdminAvatarUrlQuery, GarbageAdminAvatarUrlDto>
{
    public async Task<Result<GarbageAdminAvatarUrlDto>> HandleAsync(
        GetAssignedGarbageAdminAvatarUrlQuery request,
        CancellationToken cancellationToken)
    {
        var order = await context.GarbageOrders
            .AsNoTracking()
            .Include(x => x.AssignedGarbageAdmin)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result<GarbageAdminAvatarUrlDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        var isRequesterAssignedAdmin = order.AssignedGarbageAdminId == request.RequesterId;

        var isParticipant = isRequesterAssignedAdmin || await context.GarbageOrderUsers
            .AsNoTracking()
            .AnyAsync(x => x.GarbageOrderId == request.OrderId && x.UserId == request.RequesterId, cancellationToken);

        if (!isParticipant)
        {
            var isGroupMember = await context.UserGarbageGroups
                .AsNoTracking()
                .AnyAsync(x => x.GarbageGroupId == order.GarbageGroupId && x.UserId == request.RequesterId, cancellationToken);

            if (!isGroupMember)
            {
                return Result<GarbageAdminAvatarUrlDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
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

        return Result<GarbageAdminAvatarUrlDto>.Success(new GarbageAdminAvatarUrlDto(avatarUrl));
    }
}
