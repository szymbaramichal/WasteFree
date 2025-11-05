using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Infrastructure;

namespace WasteFree.Infrastructure.Hubs;

[Authorize(Policy = PolicyNames.GenericPolicy)]
public class GroupChatHub(
    ApplicationDataContext context,
    ICurrentUserService currentUserService,
    ILogger<GroupChatHub> logger) : Hub
{
    public async Task JoinGroup(Guid groupId)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
        {
            throw new HubException(ApiErrorCodes.InvalidUser);
        }

        var cancellationToken = Context.ConnectionAborted;
        var membership = await context.UserGarbageGroups
            .AsNoTracking()
            .Include(ug => ug.User)
            .FirstOrDefaultAsync(ug => ug.GarbageGroupId == groupId && ug.UserId == userId && !ug.IsPending, cancellationToken);

        if (membership is null)
        {
            logger.LogWarning("User {UserId} attempted to join group chat {GroupId} without membership", userId, groupId);
            throw new HubException(ApiErrorCodes.Forbidden);
        }

        if (membership.User.Role == UserRole.GarbageAdmin)
        {
            logger.LogWarning("GarbageAdmin {UserId} attempted to join group chat {GroupId}", userId, groupId);
            throw new HubException(ApiErrorCodes.Forbidden);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString(), cancellationToken);
    }

    public Task LeaveGroup(Guid groupId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
    }
}
