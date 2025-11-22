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
using WasteFree.Domain.Models;
using WasteFree.Domain.Interfaces;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record PayAdditionalUtilizationFeeCommand(
    Guid GarbageGroupId,
    Guid GarbageOrderId,
    Guid CurrentUserId) : IRequest<GarbageOrderDto>;

public sealed class PayAdditionalUtilizationFeeCommandHandler(
    ApplicationDataContext context,
    IJobSchedulerFacade jobScheduler,
    UtilizationFeeCompletionNotificationFacade completionNotificationFacade,
    IBlobStorageService blobStorageService)
    : IRequestHandler<PayAdditionalUtilizationFeeCommand, GarbageOrderDto>
{
    public async Task<Result<GarbageOrderDto>> HandleAsync(
        PayAdditionalUtilizationFeeCommand request,
        CancellationToken cancellationToken)
    {
        var garbageOrder = await context.GarbageOrders
            .Include(order => order.GarbageGroup)
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageOrderUsers)
                .ThenInclude(orderUser => orderUser.User)
            .FirstOrDefaultAsync(
                order => order.GarbageGroupId == request.GarbageGroupId && order.Id == request.GarbageOrderId,
                cancellationToken);

        if (garbageOrder is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        if (garbageOrder.AssignedGarbageAdminId is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        if (garbageOrder.GarbageOrderStatus is not GarbageOrderStatus.WaitingForUtilizationFee)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var garbageOrderUser = garbageOrder.GarbageOrderUsers
            .FirstOrDefault(user => user.UserId == request.CurrentUserId);

        if (garbageOrderUser is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
        }

        if (garbageOrderUser.HasPaidAdditionalUtilizationFee)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.PaymentAlreadyCompleted, HttpStatusCode.BadRequest);
        }

        var shareAmount = decimal.Round(garbageOrderUser.AdditionalUtilizationFeeShareAmount, 2, MidpointRounding.AwayFromZero);
        if (shareAmount <= 0m)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var userWallet = await context.Wallets
            .FirstOrDefaultAsync(wallet => wallet.UserId == request.CurrentUserId, cancellationToken);

        if (userWallet is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var shareAmountDouble = decimal.ToDouble(shareAmount);
        if (shareAmountDouble > 0 && userWallet.Funds < shareAmountDouble)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotEnoughFunds, HttpStatusCode.BadRequest);
        }

        var adminWallet = await context.Wallets
            .FirstOrDefaultAsync(wallet => wallet.UserId == garbageOrder.AssignedGarbageAdminId, cancellationToken);

        if (adminWallet is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        if (shareAmountDouble > 0)
        {
            userWallet.Funds -= shareAmountDouble;
            context.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.CreateVersion7(),
                WalletId = userWallet.Id,
                Amount = shareAmountDouble,
                TransactionType = TransactionType.GarbageExpense
            });

            adminWallet.Funds += shareAmountDouble;
            context.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.CreateVersion7(),
                WalletId = adminWallet.Id,
                Amount = shareAmountDouble,
                TransactionType = TransactionType.GarbageIncome
            });
        }

        garbageOrderUser.HasPaidAdditionalUtilizationFee = true;

        if (garbageOrder.AdditionalUtilizationFeeAmount.HasValue)
        {
            var updatedAmount = decimal.Round(
                garbageOrder.AdditionalUtilizationFeeAmount.Value - shareAmount,
                2,
                MidpointRounding.AwayFromZero);

            garbageOrder.AdditionalUtilizationFeeAmount = updatedAmount < 0m ? 0m : updatedAmount;
        }

        var isAnyOutstandingPayment = garbageOrder.GarbageOrderUsers.Any(user =>
            user.AdditionalUtilizationFeeShareAmount > 0m && !user.HasPaidAdditionalUtilizationFee);

        if (!isAnyOutstandingPayment)
        {
            garbageOrder.AdditionalUtilizationFeeAmount = 0m;
            garbageOrder.GarbageOrderStatus = GarbageOrderStatus.Completed;

            await SendCompletionNotificationsAsync(
                garbageOrder,
                completionNotificationFacade,
                jobScheduler,
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        var dto = garbageOrder.MapToGarbageOrderDto();
        if (!string.IsNullOrEmpty(dto.UtilizationProofBlobName))
        {
            dto.UtilizationProofUrl = await blobStorageService.GetReadSasUrlAsync(
                BlobContainerNames.UtilizationProofs,
                dto.UtilizationProofBlobName,
                TimeSpan.FromMinutes(60),
                cancellationToken);
        }

        return Result<GarbageOrderDto>.Success(dto);
    }

    private async Task SendCompletionNotificationsAsync(
        GarbageOrder garbageOrder,
        UtilizationFeeCompletionNotificationFacade notificationFacade,
        IJobSchedulerFacade scheduler,
        CancellationToken cancellationToken)
    {
        var requests = new List<UtilizationFeeCompletionNotificationRequest>();

        foreach (var participant in garbageOrder.GarbageOrderUsers)
        {
            if (participant.User is null)
            {
                continue;
            }

            requests.Add(new UtilizationFeeCompletionNotificationRequest(
                participant.UserId,
                participant.User.LanguagePreference,
                participant.User.Username,
                garbageOrder.GarbageGroup?.Name ?? string.Empty,
                garbageOrder.Id,
                NotificationType.UtilizationFeeCompletedParticipant));
        }

        if (garbageOrder.AssignedGarbageAdmin is { } adminUser)
        {
            requests.Add(new UtilizationFeeCompletionNotificationRequest(
                adminUser.Id,
                adminUser.LanguagePreference,
                adminUser.Username,
                garbageOrder.GarbageGroup?.Name ?? string.Empty,
                garbageOrder.Id,
                NotificationType.UtilizationFeeCompletedAdmin));
        }

        if (requests.Count == 0)
        {
            return;
        }

        var notificationContents = await notificationFacade.CreateAsync(requests, cancellationToken);
        var contentsByUser = notificationContents.ToDictionary(x => x.UserId);

        foreach (var participant in garbageOrder.GarbageOrderUsers)
        {
            if (participant.User is null || !contentsByUser.TryGetValue(participant.UserId, out var content))
            {
                continue;
            }

            await DispatchNotificationAsync(participant.User, content, InboxActionType.None, garbageOrder.Id, scheduler, cancellationToken);
        }

        if (garbageOrder.AssignedGarbageAdmin is { } admin && contentsByUser.TryGetValue(admin.Id, out var adminContent))
        {
            await DispatchNotificationAsync(admin, adminContent, InboxActionType.None, garbageOrder.Id, scheduler, cancellationToken);
        }
    }

    private async Task DispatchNotificationAsync(
        User user,
        UtilizationFeeNotificationContent notificationContent,
        InboxActionType actionType,
        Guid garbageOrderId,
        IJobSchedulerFacade scheduler,
        CancellationToken cancellationToken)
    {
        if (notificationContent.Email is not null)
        {
            await scheduler.ScheduleOneTimeJobAsync(
                nameof(OneTimeJobs.SendEmailJob),
                new SendEmailDto
                {
                    Email = user.Email,
                    Subject = notificationContent.Email.Subject,
                    Body = notificationContent.Email.Body
                },
                "Utilization fee notification",
                cancellationToken);
        }

        if (notificationContent.Inbox is not null)
        {
            context.InboxNotifications.Add(new InboxNotification
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                Title = notificationContent.Inbox.Subject,
                Message = notificationContent.Inbox.Body,
                ActionType = actionType,
                RelatedEntityId = garbageOrderId
            });
        }
    }
}
