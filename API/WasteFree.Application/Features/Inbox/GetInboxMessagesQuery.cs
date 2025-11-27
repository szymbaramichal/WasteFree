using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Inbox.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.Inbox;

public record GetInboxMessagesQuery(Guid UserId, Pager Pager)  : IRequest<ICollection<InboxMessageDto>>;

public class GetInboxMessagesQueryHandler(ApplicationDataContext context) 
    : IRequestHandler<GetInboxMessagesQuery, ICollection<InboxMessageDto>>
{
    public async Task<Result<ICollection<InboxMessageDto>>> HandleAsync(GetInboxMessagesQuery request, CancellationToken cancellationToken)
    {
        var query = context.InboxNotifications
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(x => x.CreatedDateUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Paginate(request.Pager)
            .Select(x => new InboxMessageDto
            {
                Id = x.Id,
                Title = x.Title,
                CreatedDateUtc = x.CreatedDateUtc,
                Body = x.Message,
                ActionType = x.ActionType,
                RelatedEntityId = x.RelatedEntityId
            })
            .OrderByDescending(x => x.CreatedDateUtc)
            .ToListAsync(cancellationToken);

        var pager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, totalCount);

        return PaginatedResult<ICollection<InboxMessageDto>>.PaginatedSuccess(items, pager);
    }
}
