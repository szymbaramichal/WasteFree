using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Inbox;

public record ReadInboxMessageCommand(Guid UserId, Guid MessageId) : IRequest<bool>;

public class ReadInboxMessageCommandHandler(ApplicationDataContext context) : IRequestHandler<ReadInboxMessageCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(ReadInboxMessageCommand request, CancellationToken cancellationToken)
    {
        var inboxNotification = await context.InboxNotifications.Where(x => x.UserId == request.UserId 
                                                                      && x.Id == request.MessageId && !x.IsRead)
            .FirstOrDefaultAsync(cancellationToken);

        if(inboxNotification is null)
            return Result<bool>.Failure("NOT_FOUND", HttpStatusCode.NotFound);

        inboxNotification.IsRead = true;
        
        context.InboxNotifications.Update(inboxNotification);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}