using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record MakeActionWithInvitationCommand(Guid UserId, Guid GroupId, bool MakeAction) : IRequest<bool>;

public class MakeActionWithInvitationCommandHandler(ApplicationDataContext applicationDataContext) : IRequestHandler<MakeActionWithInvitationCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(MakeActionWithInvitationCommand request, CancellationToken cancellationToken)
    {
        var pendingInvitation = await applicationDataContext.UserGarbageGroups.FirstOrDefaultAsync(x =>
            x.UserId == request.UserId &&
            x.GarbageGroupId == request.GroupId &&
            x.IsPending, cancellationToken);

        if (pendingInvitation is null)
        {
            return Result<bool>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        if (request.MakeAction)
            pendingInvitation.IsPending = false;
        else
            applicationDataContext.Remove(pendingInvitation);

        await applicationDataContext.InboxNotifications
                .Where(x => x.UserId == request.UserId 
                    && x.RelatedEntityId == pendingInvitation.GarbageGroupId 
                    && x.ActionType == Shared.Enums.InboxActionType.GroupInvitation)
                .ExecuteDeleteAsync(cancellationToken);

        await applicationDataContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}