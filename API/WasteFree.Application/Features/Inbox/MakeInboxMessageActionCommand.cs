using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Inbox;

public record MakeInboxMessageActionCommand(Guid UserId, Guid MessageId, bool MakeAction) : IRequest<bool>;

public class MakeInboxMessageActionCommandHandler(ApplicationDataContext context) : IRequestHandler<MakeInboxMessageActionCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(MakeInboxMessageActionCommand request, CancellationToken cancellationToken)
    {
        var inboxNotification = await context.InboxNotifications.Where(x => x.UserId == request.UserId 
                                                                      && x.Id == request.MessageId)
            .FirstOrDefaultAsync(cancellationToken);

        if(inboxNotification is null)
            return Result<bool>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        
        switch (inboxNotification.ActionType)
        {
            case InboxActionType.GroupInvitation:
                await AssignUserAndDeleteNotification(request, cancellationToken, inboxNotification);
                break;
            default:
                return Result<bool>.Failure(ApiErrorCodes.InvalidNotificationType, HttpStatusCode.BadRequest);
        }
        
        return Result<bool>.Success(true);
    }

    private async Task AssignUserAndDeleteNotification(MakeInboxMessageActionCommand request,
        CancellationToken cancellationToken, InboxNotification inboxNotification)
    {
        if (request.MakeAction)
            await context.UserGarbageGroups
                .Where(x => x.UserId == request.UserId && x.GarbageGroupId == inboxNotification.RelatedEntityId)
                .ExecuteUpdateAsync(x => x.SetProperty(u => u.IsPending, false),
                    cancellationToken);
        else
            await context.UserGarbageGroups
                .Where(x => x.UserId == request.UserId && x.GarbageGroupId == inboxNotification.RelatedEntityId)
                .ExecuteDeleteAsync(cancellationToken);

        context.InboxNotifications.Remove(inboxNotification);
        await context.SaveChangesAsync(cancellationToken);
    }
}