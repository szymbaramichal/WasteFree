using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Application.Jobs;
using WasteFree.Application.Jobs.Dtos;
using WasteFree.Application.Notifications.Facades;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record AcceptGarbageOrderCommand(Guid GarbageOrderId, Guid GarbageAdminId) : IRequest<GarbageOrderDto>;

public sealed class AcceptGarbageOrderCommandHandler(
    ApplicationDataContext context,
    IJobSchedulerFacade jobScheduler,
    GarbageOrderAcceptedNotificationFacade notificationFacade)
    : IRequestHandler<AcceptGarbageOrderCommand, GarbageOrderDto>
{
    public async Task<Result<GarbageOrderDto>> HandleAsync(
        AcceptGarbageOrderCommand request,
        CancellationToken cancellationToken)
    {
        var garbageOrder = await context.GarbageOrders
            .Include(order => order.GarbageGroup)
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageOrderUsers)
                .ThenInclude(orderUser => orderUser.User)
            .FirstOrDefaultAsync(order => order.Id == request.GarbageOrderId, cancellationToken);

        if (garbageOrder is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        if (garbageOrder.GarbageOrderStatus != GarbageOrderStatus.WaitingForAccept)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var garbageAdmin = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == request.GarbageAdminId, cancellationToken);

        if (garbageAdmin is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.InvalidUser, HttpStatusCode.BadRequest);
        }

        garbageOrder.GarbageOrderStatus = GarbageOrderStatus.WaitingForPickup;
        garbageOrder.AssignedGarbageAdminId = request.GarbageAdminId;

        await SendAcceptanceNotificationsAsync(
            garbageOrder,
            garbageAdmin,
            notificationFacade,
            jobScheduler,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(garbageOrder)
            .Reference(order => order.AssignedGarbageAdmin)
            .LoadAsync(cancellationToken);

        return Result<GarbageOrderDto>.Success(garbageOrder.MapToGarbageOrderDto());
    }

    private async Task SendAcceptanceNotificationsAsync(
        GarbageOrder garbageOrder,
        User garbageAdmin,
        GarbageOrderAcceptedNotificationFacade notificationFacade,
        IJobSchedulerFacade scheduler,
        CancellationToken cancellationToken)
    {
        var participants = garbageOrder.GarbageOrderUsers
            .Where(orderUser => orderUser.User is not null)
            .ToList();

        if (participants.Count == 0)
        {
            return;
        }

        var groupName = garbageOrder.GarbageGroup?.Name ?? string.Empty;

        var notificationRequests = participants
            .Select(participant => new GarbageOrderAcceptedNotificationRequest(
                participant.UserId,
                participant.User!.LanguagePreference,
                participant.User.Username,
                groupName,
                garbageOrder.PickupDate,
                garbageAdmin.Username))
            .ToList();

        var notificationContents = await notificationFacade.CreateAsync(notificationRequests, cancellationToken);
        var contentsByUser = notificationContents.ToDictionary(x => x.UserId);

        foreach (var participant in participants)
        {
            if (!contentsByUser.TryGetValue(participant.UserId, out var content))
            {
                continue;
            }

            if (content.Email is not null)
            {
                await scheduler.ScheduleOneTimeJobAsync(
                    nameof(OneTimeJobs.SendEmailJob),
                    new SendEmailDto
                    {
                        Email = participant.User!.Email,
                        Subject = content.Email.Subject,
                        Body = content.Email.Body
                    },
                    "Garbage order accepted email",
                    cancellationToken);
            }

            if (content.Inbox is not null)
            {
                context.InboxNotifications.Add(new InboxNotification
                {
                    Id = Guid.CreateVersion7(),
                    UserId = participant.UserId,
                    Title = content.Inbox.Subject,
                    Message = content.Inbox.Body,
                    ActionType = InboxActionType.None,
                    RelatedEntityId = garbageOrder.Id
                });
            }
        }
    }
}
