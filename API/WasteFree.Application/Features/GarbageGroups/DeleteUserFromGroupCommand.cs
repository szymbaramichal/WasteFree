using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.GarbageGroups;

public record DeleteUserFromGroupCommand(Guid GroupId, Guid CurrentUserId, Guid UserToRemoveId) : IRequest<bool>;

public class DeleteUserFromGroupCommandHandler(ApplicationDataContext context) : IRequestHandler<DeleteUserFromGroupCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(DeleteUserFromGroupCommand request, CancellationToken cancellationToken)
    {
        var userGroupInfo = await context.UserGarbageGroups
            .FilterNonPrivate()
            .Include(x => x.GarbageGroup)
            .FirstOrDefaultAsync(x => x.UserId == request.CurrentUserId && x.GarbageGroupId == request.GroupId
                                                                            && x.Role == GarbageGroupRole.Owner, cancellationToken);
        
        if (userGroupInfo is null)
            return Result<bool>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);

        await context.UserGarbageGroups
            .Where(x => x.UserId == request.UserToRemoveId && x.GarbageGroupId == request.GroupId)
            .ExecuteDeleteAsync(cancellationToken);

        await context.InboxNotifications
            .Where(x => x.UserId == request.UserToRemoveId 
                        && x.ActionType == InboxActionType.GroupInvitation
                        && x.RelatedEntityId == request.GroupId)
            .ExecuteDeleteAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}