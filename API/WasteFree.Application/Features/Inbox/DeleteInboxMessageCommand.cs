using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Inbox;

public record DeleteInboxMessageComamnd(Guid UserId, Guid MessageId) : IRequest<bool>;

public class DeleteInboxMessageComamndHandler(ApplicationDataContext context) : IRequestHandler<DeleteInboxMessageComamnd, bool>
{
    public async Task<Result<bool>> HandleAsync(DeleteInboxMessageComamnd request, CancellationToken cancellationToken)
    {
        int rows = await context.InboxNotifications.Where(x => x.UserId == request.UserId 
                                                               && x.Id == request.MessageId)
            .ExecuteDeleteAsync(cancellationToken);
        
        if(rows > 0)
            return Result<bool>.Success(true);
        
    return Result<bool>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        
    }
}