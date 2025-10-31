using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageGroupOrders.Dtos;
using WasteFree.Application.Helpers;
using WasteFree.Application.Jobs;
using WasteFree.Application.Jobs.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Hubs;

namespace WasteFree.Application.Features.GarbageGroupOrders;

public record GarbageGroupOrderCommand (
    Guid GarbageGroupId,
    Guid CurrentUserId,
    PickupOption PickupOption,
    ICollection<Guid> UserIds,
    ContainerSize? ContainerSize,
    DateTime? DropOffDate,
    DateTime PickupDate,
    bool IsHighPriority,
    bool CollectingService) : IRequest<GarbageGroupOrderDto>;

public class GarbageGroupOrderCommandHandler(
    ApplicationDataContext context,
    IHubContext<NotificationHub> hubContext,
    IJobSchedulerFacade jobScheduler) 
    : IRequestHandler<GarbageGroupOrderCommand, GarbageGroupOrderDto>
{
    public async Task<Result<GarbageGroupOrderDto>> HandleAsync(GarbageGroupOrderCommand request, CancellationToken cancellationToken)
    {
        var userGroup = await context.UserGarbageGroups
            .AsNoTracking()
            .Include(x => x.GarbageGroup)
            .FirstOrDefaultAsync(x => x.GarbageGroupId == request.GarbageGroupId
                && x.UserId == request.CurrentUserId && x.Role == GarbageGroupRole.Owner, cancellationToken);
        
        if (userGroup is null)
            return Result<GarbageGroupOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);
        
        var groupUsers = await context.UserGarbageGroups
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.GarbageGroupId == request.GarbageGroupId)
            .ToListAsync(cancellationToken);
        
        var groupUserIds = groupUsers.Select(x => x.UserId).ToHashSet();
        
        if (!request.UserIds.All(id => groupUserIds.Contains(id)))
            return Result<GarbageGroupOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);

        var garbageOrderId = Guid.CreateVersion7();
        var garbageOrder = new GarbageOrder
        {
            Id = garbageOrderId,
            PickupOption = request.PickupOption,
            ContainerSize = request.ContainerSize,
            DropOffDate = request.DropOffDate,
            PickupDate = request.PickupDate,
            IsHighPriority = request.IsHighPriority,
            CollectingService = request.CollectingService,
            GarbageOrderStatus = GarbageOrderStatus.Created,
            GarbageGroupId = request.GarbageGroupId
        };
        
        var connectionIds = new HashSet<string>();

        var notificationTemplates = await context
            .NotificationTemplates.Where(x => x.Type == NotificationType.GarbageOrderCreated)
            .ToListAsync(cancellationToken);

        foreach (var userId in request.UserIds)
        {
            garbageOrder.GarbageOrderUsers.Add(new GarbageOrderUsers
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                GarbageOrderId = garbageOrder.Id,
            });

            var connectionId = NotificationHub.GetConnectionId(userId);
            if (connectionId != null)
            {
                connectionIds.Add(connectionId);
            }

            var user = groupUsers.First(x => x.UserId == userId);
            var emailTemplate = notificationTemplates
                .FirstOrDefault(x =>
                    x.LanguagePreference == user.User.LanguagePreference && x.Channel == NotificationChannel.Email);

            if (emailTemplate != null)
            {
                var emailBody = EmailTemplateHelper.ApplyPlaceholders(emailTemplate.Body,
                    new Dictionary<string, string>
                    {
                        { "Username", user.User.Username },
                        { "GroupName", userGroup.GarbageGroup.Name },
                        { "PickupDate", request.PickupDate.ToString("dd-MM-yyyy") },
                    });

                await jobScheduler.ScheduleOneTimeJobAsync(nameof(OneTimeJobs.SendEmailJob),
                    new SendEmailDto
                    {
                        Email = user.User.Email,
                        Subject = emailTemplate.Subject,
                        Body = emailBody
                    },
                    "Garbage order email",
                    cancellationToken);
            }

            var inboxTemplate = notificationTemplates
                .FirstOrDefault(x =>
                    x.LanguagePreference == user.User.LanguagePreference && x.Channel == NotificationChannel.Inbox);
            
            if (inboxTemplate is not null)
            {
                var inboxBody = EmailTemplateHelper.ApplyPlaceholders(inboxTemplate.Body,
                    new Dictionary<string, string>
                    {
                        { "Username", user.User.Username },
                        { "GroupName", userGroup.GarbageGroup.Name },
                        { "PickupDate", request.PickupDate.ToString("dd-MM-yyyy") },
                    });
                
                await context.InboxNotifications.AddAsync(new InboxNotification
                {
                    ActionType = InboxActionType.GroupInvitation,
                    Title = inboxTemplate.Subject,
                    Message = inboxBody,
                    UserId = user.User.Id,
                    RelatedEntityId = garbageOrderId
                }, cancellationToken);

            }
        }

        if (connectionIds.Count > 0)
        {
            await hubContext.Clients.Clients(connectionIds).SendAsync(
                SignalRMethods.InvitedToGarbageOrder,
                string.Empty,
                cancellationToken);
        }
        
        context.Add(garbageOrder);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result<GarbageGroupOrderDto>.Success(garbageOrder.MapToGarbageGroupOrderDto());
    }
}