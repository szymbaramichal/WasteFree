using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Application.Jobs;
using WasteFree.Application.Jobs.Dtos;
using WasteFree.Application.Notifications.Facades;
using WasteFree.Application.Services.GarbageOrders;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Hubs;

namespace WasteFree.Application.Features.GarbageOrders;

public record GarbageOrderCommand(
    Guid GarbageGroupId,
    Guid CurrentUserId,
    PickupOption PickupOption,
    ICollection<Guid> UserIds,
    ContainerSize? ContainerSize,
    DateTime? DropOffDate,
    DateTime PickupDate,
    bool IsHighPriority,
    bool CollectingService) : IRequest<GarbageOrderDto>;

public class GarbageOrderCommandHandler(
    ApplicationDataContext context,
    IHubContext<NotificationHub> hubContext,
    IJobSchedulerFacade jobScheduler,
    GarbageOrderCreatedNotificationFacade notificationFacade,
    IGarbageOrderCostCalculator costCalculator)
    : IRequestHandler<GarbageOrderCommand, GarbageOrderDto>
{
    public async Task<Result<GarbageOrderDto>> HandleAsync(GarbageOrderCommand request, CancellationToken cancellationToken)
    {
        var userGroup = await context.UserGarbageGroups
            .AsNoTracking()
            .Include(x => x.GarbageGroup)
            .FirstOrDefaultAsync(x => x.GarbageGroupId == request.GarbageGroupId
                && x.UserId == request.CurrentUserId && x.Role == GarbageGroupRole.Owner, cancellationToken);
        
        if (userGroup is null)
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);
        
        var groupUsers = await context.UserGarbageGroups
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.GarbageGroupId == request.GarbageGroupId)
            .ToListAsync(cancellationToken);
        
        var groupUserIds = groupUsers.Select(x => x.UserId).ToHashSet();
        
        if (!request.UserIds.All(id => groupUserIds.Contains(id)))
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);

        var userIdList = request.UserIds.ToList();
        var userCount = userIdList.Count;

        var costBreakdown = costCalculator.CalculateEstimate(
            request.PickupOption,
            request.ContainerSize,
            request.DropOffDate,
            request.PickupDate,
            request.IsHighPriority,
            request.CollectingService);
        var totalCostWithUtilization = costBreakdown.TotalCost; // includes 25% prepaid utilization fee
        var totalCents = userCount == 0
            ? 0L
            : (long)decimal.Round(totalCostWithUtilization * 100m, 0, MidpointRounding.AwayFromZero);
        var baseCents = userCount == 0 ? 0L : totalCents / userCount;
        var remainderCents = userCount == 0 ? 0L : totalCents % userCount;
        var orderCost = totalCents / 100m;

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
            GarbageOrderStatus = GarbageOrderStatus.WaitingForPayment,
            GarbageGroupId = request.GarbageGroupId,
            Cost = orderCost,
            PrepaidUtilizationFeeAmount = costBreakdown.PrepaidUtilizationFee
        };
        
        var connectionIds = new HashSet<string>();

        var notificationRequests = userIdList
            .Select(userId =>
            {
                var user = groupUsers.First(x => x.UserId == userId);
                return new GarbageOrderNotificationRequest(
                    userId,
                    user.User.LanguagePreference,
                    user.User.Username,
                    userGroup.GarbageGroup.Name,
                    request.PickupDate);
            })
            .ToList();

        var notificationContents = await notificationFacade
            .CreateAsync(notificationRequests, cancellationToken);

        var notificationsByUserId = notificationContents.ToDictionary(x => x.UserId);

        for (var index = 0; index < userIdList.Count; index++)
        {
            var userId = userIdList[index];
            var shareCents = baseCents + (index < remainderCents ? 1 : 0);
            var shareAmount = shareCents / 100m;

            garbageOrder.GarbageOrderUsers.Add(new GarbageOrderUsers
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                GarbageOrderId = garbageOrder.Id,
                ShareAmount = shareAmount,
                HasAcceptedPayment = shareAmount == 0m
            });

            var connectionId = NotificationHub.GetConnectionId(userId);
            if (connectionId != null)
            {
                connectionIds.Add(connectionId);
            }

            var user = groupUsers.First(x => x.UserId == userId);
            notificationsByUserId.TryGetValue(userId, out var notificationContent);

            if (notificationContent?.Email is not null)
            {
                await jobScheduler.ScheduleOneTimeJobAsync(nameof(OneTimeJobs.SendEmailJob),
                    new SendEmailDto
                    {
                        Email = user.User.Email,
                        Subject = notificationContent.Email.Subject,
                        Body = notificationContent.Email.Body
                    },
                    "Garbage order email",
                    cancellationToken);
            }

            if (notificationContent?.Inbox is not null)
            {
                await context.InboxNotifications.AddAsync(new InboxNotification
                {
                    ActionType = InboxActionType.GroupInvitation,
                    Title = notificationContent.Inbox.Subject,
                    Message = notificationContent.Inbox.Body,
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
        
        return Result<GarbageOrderDto>.Success(garbageOrder.MapToGarbageOrderDto());
    }
}
