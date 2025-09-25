using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Features.Inbox.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.Inbox;

public record GetInboxCounterQuery(Guid UserId) : IRequest<InboxCounterDto>;

public class GetInboxCounterQueryHandler(ApplicationDataContext context) : IRequestHandler<GetInboxCounterQuery, InboxCounterDto>
{
    public async Task<Result<InboxCounterDto>> HandleAsync(GetInboxCounterQuery request, CancellationToken cancellationToken)
    {
        var unreadCount = await context.InboxNotifications.Where(x => x.UserId == request.UserId && !x.IsRead)
            .CountAsync(cancellationToken);
        
        return Result<InboxCounterDto>.Success(new InboxCounterDto
        {
            UnreadMessages = unreadCount
        });
    }
}