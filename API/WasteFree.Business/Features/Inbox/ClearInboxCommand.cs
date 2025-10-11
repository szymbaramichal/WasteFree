using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Infrastructure;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Inbox;

public record ClearInboxCommand(Guid UserId) : IRequest<bool>;

public class ClearInboxCommandHandler(ApplicationDataContext context) : IRequestHandler<ClearInboxCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(ClearInboxCommand request, CancellationToken cancellationToken)
    {
        int rows = await context.InboxNotifications.Where(x => x.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}