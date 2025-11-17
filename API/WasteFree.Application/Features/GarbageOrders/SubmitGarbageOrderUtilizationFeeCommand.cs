using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public sealed record SubmitGarbageOrderUtilizationFeeCommand(
    Guid OrderId,
    Guid GarbageAdminId,
    decimal UtilizationFeeAmount,
    IFormFile UtilizationProof) : IRequest<GarbageOrderDto>;

public sealed class SubmitGarbageOrderUtilizationFeeCommandHandler(
    ApplicationDataContext context,
    IBlobStorageService blobStorageService)
    : IRequestHandler<SubmitGarbageOrderUtilizationFeeCommand, GarbageOrderDto>
{
    private const long MaxProofBytes = 10 * 1024 * 1024; // 10 MB
    private const decimal UtilizationFeeMultiplier = 1.25m;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public async Task<Result<GarbageOrderDto>> HandleAsync(
        SubmitGarbageOrderUtilizationFeeCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UtilizationFeeAmount < 0)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        if (request.UtilizationProof is null || request.UtilizationProof.Length == 0)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.EmptyImage, HttpStatusCode.BadRequest);
        }

        if (request.UtilizationProof.Length > MaxProofBytes)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.TooBigImage, HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.UtilizationProof.ContentType) ||
            !AllowedContentTypes.Contains(request.UtilizationProof.ContentType))
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.UnsupportedImageType, HttpStatusCode.BadRequest);
        }

        var garbageOrder = await context.GarbageOrders
            .Include(order => order.GarbageGroup)
            .Include(order => order.AssignedGarbageAdmin)
            .Include(order => order.GarbageOrderUsers)
                .ThenInclude(orderUser => orderUser.User)
            .FirstOrDefaultAsync(order => order.Id == request.OrderId, cancellationToken);

        if (garbageOrder is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.NotFound);
        }

        if (garbageOrder.AssignedGarbageAdminId is null ||
            garbageOrder.AssignedGarbageAdminId != request.GarbageAdminId)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
        }

        if (garbageOrder.GarbageOrderStatus is not GarbageOrderStatus.WaitingForPickup)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var proofExtension = Path.GetExtension(request.UtilizationProof.FileName);
        if (string.IsNullOrWhiteSpace(proofExtension))
        {
            proofExtension = request.UtilizationProof.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".bin"
            };
        }

        var blobName = $"{garbageOrder.Id}_utilization_{DateTime.UtcNow:yyyyMMddHHmmssfff}{proofExtension}";

        await using (var proofStream = request.UtilizationProof.OpenReadStream())
        {
            await blobStorageService.UploadAsync(
                proofStream,
                request.UtilizationProof.ContentType,
                BlobContainerNames.UtilizationProofs,
                blobName,
                cancellationToken);
        }

        var normalizedUtilizationFee = decimal.Round(request.UtilizationFeeAmount, 2, MidpointRounding.AwayFromZero);

        var totalCost = garbageOrder.Cost;

        var computedBaseCost = totalCost <= 0m
            ? 0m
            : decimal.Round(totalCost / UtilizationFeeMultiplier, 2, MidpointRounding.AwayFromZero);
        var computedPrepaidFee = decimal.Round(totalCost - computedBaseCost, 2, MidpointRounding.AwayFromZero);

        var prepaidUtilizationFee = garbageOrder.PrepaidUtilizationFeeAmount > 0m
            ? decimal.Round(garbageOrder.PrepaidUtilizationFeeAmount, 2, MidpointRounding.AwayFromZero)
            : computedPrepaidFee;
        var baseCost = garbageOrder.PrepaidUtilizationFeeAmount > 0m
            ? decimal.Round(totalCost - prepaidUtilizationFee, 2, MidpointRounding.AwayFromZero)
            : computedBaseCost;

        if (baseCost < 0m)
        {
            baseCost = 0m;
        }

        garbageOrder.UtilizationFeeAmount = normalizedUtilizationFee;
        garbageOrder.UtilizationProofBlobName = blobName;
        garbageOrder.UtilizationFeeSubmittedDateUtc = DateTime.UtcNow;
        garbageOrder.AdditionalUtilizationFeeAmount = null;
        garbageOrder.GarbageOrderStatus = GarbageOrderStatus.WaitingForUtilizationFee;

        if (normalizedUtilizationFee <= prepaidUtilizationFee)
        {
            var adminWallet = await context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.GarbageAdminId, cancellationToken);

            if (adminWallet is null)
            {
                return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
            }

            if (normalizedUtilizationFee > 0m)
            {
                var adminPayout = Math.Round((double)normalizedUtilizationFee, 2, MidpointRounding.AwayFromZero);
                adminWallet.Funds += adminPayout;

                context.WalletTransactions.Add(new WalletTransaction
                {
                    Id = Guid.CreateVersion7(),
                    WalletId = adminWallet.Id,
                    Amount = adminPayout,
                    TransactionType = TransactionType.GarbageIncome
                });
            }

            var refundAmount = decimal.Round(prepaidUtilizationFee - normalizedUtilizationFee, 2, MidpointRounding.AwayFromZero);
            if (refundAmount > 0m)
            {
                var userIds = garbageOrder.GarbageOrderUsers
                    .Select(u => u.UserId)
                    .Distinct()
                    .ToList();

                var wallets = await context.Wallets
                    .Where(w => userIds.Contains(w.UserId))
                    .ToDictionaryAsync(w => w.UserId, cancellationToken);

                if (wallets.Count != userIds.Count)
                {
                    return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
                }

                var refunds = CalculateRefundDistribution(garbageOrder.GarbageOrderUsers, refundAmount);

                foreach (var (userId, refundCents) in refunds)
                {
                    if (refundCents <= 0)
                    {
                        continue;
                    }

                    if (!wallets.TryGetValue(userId, out var wallet))
                    {
                        return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
                    }

                    var refundValue = refundCents / 100d;
                    wallet.Funds += refundValue;

                    context.WalletTransactions.Add(new WalletTransaction
                    {
                        Id = Guid.CreateVersion7(),
                        WalletId = wallet.Id,
                        Amount = refundValue,
                        TransactionType = TransactionType.Refund
                    });
                }
            }

            foreach (var participant in garbageOrder.GarbageOrderUsers)
            {
                participant.AdditionalUtilizationFeeShareAmount = 0m;
            }

            garbageOrder.AdditionalUtilizationFeeAmount = 0m;
            garbageOrder.GarbageOrderStatus = GarbageOrderStatus.Completed;
        }
        else
        {
            var outstandingAmount = decimal.Round(normalizedUtilizationFee - prepaidUtilizationFee, 2, MidpointRounding.AwayFromZero);
            garbageOrder.AdditionalUtilizationFeeAmount = outstandingAmount;

            var outstandingDistribution = AllocateAmountByShareInCents(garbageOrder.GarbageOrderUsers, outstandingAmount);

            foreach (var participant in garbageOrder.GarbageOrderUsers)
            {
                outstandingDistribution.TryGetValue(participant.UserId, out var shareCents);
                var shareAmount = shareCents / 100m;
                participant.AdditionalUtilizationFeeShareAmount = shareAmount;

                var outstandingMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Additional utilization fee of {0:F2} is required for order {1}. Your share is {2:F2}.",
                    outstandingAmount,
                    garbageOrder.Id,
                    shareAmount);

                // TODO: Trigger additional payment workflow once the flow is designed.
                context.InboxNotifications.Add(new InboxNotification
                {
                    Id = Guid.CreateVersion7(),
                    UserId = participant.UserId,
                    Title = "Utilization fee pending",
                    Message = outstandingMessage,
                    ActionType = InboxActionType.MakePayment,
                    RelatedEntityId = garbageOrder.Id
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<GarbageOrderDto>.Success(garbageOrder.MapToGarbageOrderDto());
    }

    private static IReadOnlyDictionary<Guid, long> AllocateAmountByShareInCents(
        IEnumerable<GarbageOrderUsers> users,
        decimal amount)
    {
        var amountCents = (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        if (amountCents <= 0)
        {
            return new Dictionary<Guid, long>();
        }

        var orderedUsers = users
            .OrderByDescending(u => u.ShareAmount)
            .ThenBy(u => u.Id)
            .ToList();

        var totalShareAmount = orderedUsers.Sum(u => u.ShareAmount);
        if (totalShareAmount <= 0m)
        {
            return new Dictionary<Guid, long>();
        }

        var allocations = new Dictionary<Guid, long>(orderedUsers.Count);
        long allocated = 0;

        foreach (var user in orderedUsers)
        {
            var rawShare = (decimal)amountCents * (user.ShareAmount / totalShareAmount);
            var cents = (long)Math.Floor(rawShare);
            allocations[user.UserId] = cents;
            allocated += cents;
        }

        var remainder = amountCents - allocated;
        for (var index = 0; index < orderedUsers.Count && remainder > 0; index++)
        {
            var userId = orderedUsers[index].UserId;
            allocations[userId] += 1;
            remainder--;
        }

        return allocations;
    }

    private static IReadOnlyDictionary<Guid, long> CalculateRefundDistribution(
        IEnumerable<GarbageOrderUsers> users,
        decimal refundAmount) => AllocateAmountByShareInCents(users, refundAmount);
}
