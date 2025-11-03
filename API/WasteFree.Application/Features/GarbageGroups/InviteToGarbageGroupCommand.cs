using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Jobs;
using WasteFree.Application.Jobs.Dtos;
using WasteFree.Application.Notifications.Facades;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Extensions;
using WasteFree.Infrastructure.Hubs;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Application.Features.GarbageGroups;

public record InviteToGarbageGroupCommand(Guid GroupId, string UsernameToInvite) : IRequest<bool>;

public class InviteToGarbageGroupCommandHandler(ApplicationDataContext context, 
    ICurrentUserService currentUserService,
    IJobSchedulerFacade jobScheduler,
    IHubContext<NotificationHub> hubContext,
    GarbageGroupInvitationNotificationFacade invitationNotificationFacade) : IRequestHandler<InviteToGarbageGroupCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(InviteToGarbageGroupCommand request, CancellationToken cancellationToken)
    {
        var userGroupInfo = await context.UserGarbageGroups
            .FilterNonPrivate()
            .Include(x => x.GarbageGroup)
            .FirstOrDefaultAsync(x => x.UserId == currentUserService.UserId && x.GarbageGroupId == request.GroupId
                                                                            && x.Role == GarbageGroupRole.Owner, cancellationToken);
        
        if (userGroupInfo is null)
            return Result<bool>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        
        var userToAdd = await context.Users
            .FirstOrDefaultAsync(x => x.Username.ToLower() == request.UsernameToInvite.ToLower(), cancellationToken);
        
        if (userToAdd is null)
            return Result<bool>.Failure(ApiErrorCodes.InvitedUserNotFound, HttpStatusCode.NotFound);

        if (userToAdd.Role != UserRole.User)
            return Result<bool>.Failure(ApiErrorCodes.InvitedUserNotFound, HttpStatusCode.NotFound);
        
        var alreadyInGroup = await context.UserGarbageGroups
            .AnyAsync(x => x.UserId == userToAdd.Id && x.GarbageGroupId == request.GroupId, cancellationToken);
        
        if(alreadyInGroup)
            return Result<bool>.Failure(ApiErrorCodes.AlreadyInGroup, HttpStatusCode.BadRequest);

        var userGarbageGroup = new UserGarbageGroup
        {
            Role = GarbageGroupRole.User,
            UserId = userToAdd.Id,
            GarbageGroupId = request.GroupId,
            IsPending = true
        };
        
        await context.UserGarbageGroups.AddAsync(userGarbageGroup, cancellationToken);

        var notificationContent = await invitationNotificationFacade.CreateAsync(
            userToAdd.LanguagePreference,
            userToAdd.Username,
            currentUserService.Username,
            userGroupInfo.GarbageGroup.Name,
            cancellationToken);

        if (notificationContent is null)
            return Result<bool>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);

        await SendNotifications(userToAdd, userGroupInfo, notificationContent, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    private async Task SendNotifications(
        User userToAdd,
        UserGarbageGroup userGroupInfo,
        GarbageGroupInvitationNotificationContent notificationContent,
        CancellationToken cancellationToken)
    {
        if (notificationContent.Email is not null)
        {
            await jobScheduler.ScheduleOneTimeJobAsync(
                nameof(OneTimeJobs.SendEmailJob),
                new SendEmailDto
                {
                    Email = userToAdd.Email,
                    Subject = notificationContent.Email.Subject,
                    Body = notificationContent.Email.Body
                },
                "Send group invitation email",
                cancellationToken);
        }

        var connectionId = NotificationHub.GetConnectionId(userToAdd.Id);

        if (notificationContent.Inbox is not null)
        {
            await context.InboxNotifications.AddAsync(new InboxNotification
            {
                ActionType = InboxActionType.GroupInvitation,
                Title = notificationContent.Inbox.Subject,
                Message = notificationContent.Inbox.Body,
                UserId = userToAdd.Id,
                RelatedEntityId = userGroupInfo.GarbageGroupId
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            var inboxCounter = await context.InboxNotifications.CountAsync(
                x => x.UserId == userToAdd.Id,
                cancellationToken);

            if (connectionId != null)
            {
                await hubContext.Clients.Client(connectionId)
                    .SendAsync(SignalRMethods.UpdateInboxCounter, $"{inboxCounter}", cancellationToken);
            }
            return;
        }

        if (connectionId != null)
        {
            await hubContext.Clients.Client(connectionId)
                .SendAsync(SignalRMethods.UpdateInboxCounter, string.Empty, cancellationToken);
        }
    }
}