using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Inbox.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Inbox;

public record GetInboxCounterQuery(Guid UserId) : IRequest<InboxCounterDto>;

public class GetInboxCounterQueryHandler(ApplicationDataContext context) : IRequestHandler<GetInboxCounterQuery, InboxCounterDto>
{
    public async Task<Result<InboxCounterDto>> HandleAsync(GetInboxCounterQuery request, CancellationToken cancellationToken)
    {
        var unreadCount = await context.InboxNotifications.Where(x => x.UserId == request.UserId)
            .CountAsync(cancellationToken);
        
        return Result<InboxCounterDto>.Success(new InboxCounterDto
        {
            UnreadMessages = unreadCount
        });
    }
}