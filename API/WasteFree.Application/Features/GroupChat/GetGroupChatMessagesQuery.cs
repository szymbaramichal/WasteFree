using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GroupChat.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;

namespace WasteFree.Application.Features.GroupChat;

public record GetGroupChatMessagesQuery(Guid UserId, Guid GroupId, Pager Pager) : IRequest<ICollection<GroupChatMessageDto>>;

public class GetGroupChatMessagesQueryHandler(ApplicationDataContext context)
    : IRequestHandler<GetGroupChatMessagesQuery, ICollection<GroupChatMessageDto>>
{
    public async Task<Result<ICollection<GroupChatMessageDto>>> HandleAsync(GetGroupChatMessagesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result<ICollection<GroupChatMessageDto>>.Failure(ApiErrorCodes.InvalidUser, HttpStatusCode.Unauthorized);
        }

        var membership = await context.UserGarbageGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ug => ug.GarbageGroupId == request.GroupId && ug.UserId == request.UserId,
                cancellationToken);

        if (membership is null)
        {
            var groupExists = await context.GarbageGroups
                .AsNoTracking()
                .AnyAsync(g => g.Id == request.GroupId, cancellationToken);

            var code = groupExists ? ApiErrorCodes.Forbidden : ApiErrorCodes.NotFound;
            var status = groupExists ? HttpStatusCode.Forbidden : HttpStatusCode.NotFound;

            return Result<ICollection<GroupChatMessageDto>>.Failure(code, status);
        }

        if (membership.IsPending)
        {
            return Result<ICollection<GroupChatMessageDto>>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
        }

        var baseQuery = context.GarbageGroupMessages
            .AsNoTracking()
            .Where(m => m.GarbageGroupId == request.GroupId)
            .Include(m => m.User)
            .OrderByDescending(m => m.CreatedDateUtc);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Paginate(request.Pager)
            .Select(m => new { Message = m, m.User })
            .ToListAsync(cancellationToken);

        var dtoItems = items
            .Select(x => x.Message.ToDto(x.User))
            .OrderBy(x => x.SentAtUtc)
            .ToList();

        var pager = new Pager(request.Pager.PageNumber, request.Pager.PageSize, totalCount);

        return PaginatedResult<ICollection<GroupChatMessageDto>>.PaginatedSuccess(dtoItems, pager);
    }
}
