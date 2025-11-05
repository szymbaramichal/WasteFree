using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GroupChat.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Hubs;

namespace WasteFree.Application.Features.GroupChat;

public record CreateGroupChatMessageCommand(Guid UserId, Guid GroupId, string Content) : IRequest<GroupChatMessageDto>;

public class CreateGroupChatMessageCommandHandler(
    ApplicationDataContext context,
    IHubContext<GroupChatHub> hubContext) : IRequestHandler<CreateGroupChatMessageCommand, GroupChatMessageDto>
{
    public async Task<Result<GroupChatMessageDto>> HandleAsync(CreateGroupChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            return Result<GroupChatMessageDto>.Failure(ApiErrorCodes.InvalidUser, HttpStatusCode.Unauthorized);
        }

        var trimmedContent = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedContent))
        {
            return Result<GroupChatMessageDto>.Failure(ValidationErrorCodes.ChatMessageRequired, HttpStatusCode.BadRequest);
        }

        if (trimmedContent.Length > 2000)
        {
            return Result<GroupChatMessageDto>.Failure(ValidationErrorCodes.ChatMessageTooLong, HttpStatusCode.BadRequest);
        }

        var membership = await context.UserGarbageGroups
            .Include(ug => ug.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ug => ug.GarbageGroupId == request.GroupId && ug.UserId == request.UserId,
                cancellationToken);

        if (membership is null)
        {
            return Result<GroupChatMessageDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        if (membership.IsPending)
        {
            return Result<GroupChatMessageDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
        }

        var message = new WasteFree.Domain.Entities.GarbageGroupMessage
        {
            Id = Guid.NewGuid(),
            GarbageGroupId = request.GroupId,
            UserId = request.UserId,
            Content = trimmedContent
        };

        await context.GarbageGroupMessages.AddAsync(message, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var dto = message.ToDto(membership.User);

        await hubContext.Clients
            .Group(request.GroupId.ToString())
            .SendAsync(SignalRMethods.ReceiveGroupMessage, dto, cancellationToken);

        return Result<GroupChatMessageDto>.Success(dto);
    }
}
