using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WasteFree.Business.Abstractions.Messaging;
using WasteFree.Business.Helpers;
using WasteFree.Business.Jobs;
using WasteFree.Business.Jobs.Dtos;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Hubs;
using WasteFree.Shared.Constants;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;
using WasteFree.Shared.Interfaces;
using WasteFree.Shared.Models;

namespace WasteFree.Business.Features.GarbageGroups;

public record InviteToGarbageGroupCommand(Guid GroupId, string UsernameToInvite) : IRequest<bool>;

public class InviteToGarbageGroupCommandHandler(ApplicationDataContext context, 
    ICurrentUserService currentUserService,
    IJobSchedulerFacade jobScheduler,
    IHubContext<NotificationHub> hubContext) : IRequestHandler<InviteToGarbageGroupCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(InviteToGarbageGroupCommand request, CancellationToken cancellationToken)
    {
        var userGroupInfo = await context.UserGarbageGroups
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

        await SendNotifications(request, userToAdd, userGroupInfo, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    private async Task SendNotifications(InviteToGarbageGroupCommand request,
        User userToAdd, UserGarbageGroup userGroupInfo, CancellationToken cancellationToken)
    {
        var notificationTemplate = await context.NotificationTemplates
            .FirstOrDefaultAsync(x => x.LanguagePreference == userToAdd.LanguagePreference
                                      && x.Type == NotificationType.GarbageGroupInvitation
                                      && x.Channel == NotificationChannel.Inbox, cancellationToken);
        
        var personalizedBody = EmailTemplateHelper.ApplyPlaceholders(notificationTemplate!.Body, new Dictionary<string, string>
        {
            {"RecipientUsername", userToAdd.Username},
            {"SenderUsername", currentUserService.Username},
            {"GroupName", userGroupInfo.GarbageGroup.Name},
        });
        
        var personalizedSubject = EmailTemplateHelper.ApplyPlaceholders(notificationTemplate!.Subject, new Dictionary<string, string>
        {
            {"GroupName", userGroupInfo.GarbageGroup.Name},
        });
        
        await jobScheduler.ScheduleOneTimeJobAsync(nameof(OneTimeJobs.SendEmailJob), 
            new SendEmailDto
            {
                Email = userToAdd.Email,
                Subject = personalizedSubject,
                Body = personalizedBody
            },
            "Send group invitation email", 
            cancellationToken);

        await context.InboxNotifications.AddAsync(new InboxNotification
        {
            ActionType = InboxActionType.GroupInvitation,
            Title = personalizedSubject,
            Message = personalizedBody,
            UserId = userToAdd.Id,
            RelatedEntityId = userGroupInfo.GarbageGroupId
        }, cancellationToken);

        var inboxCounter = await context.InboxNotifications.FirstOrDefaultAsync(x => x.UserId == userToAdd.Id, 
            cancellationToken);
        
        var connectionId = NotificationHub.GetConnectionId(userToAdd.Id);
        if (connectionId != null)
        {
            await hubContext.Clients.Client(connectionId).SendAsync(SignalRMethods.UpdateInboxCounter,
                $"{inboxCounter}", cancellationToken);
        }
    }
}