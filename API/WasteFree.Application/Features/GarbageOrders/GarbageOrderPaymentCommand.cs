using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.GarbageOrders.Dtos;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.GarbageOrders;

public record GarbageOrderPaymentCommand(
    Guid GarbageGroupId,
    Guid GarbageOrderId,
    Guid CurrentUserId) : IRequest<GarbageOrderDto>;

public class GarbageOrderPaymentCommandHandler(ApplicationDataContext context)
    : IRequestHandler<GarbageOrderPaymentCommand, GarbageOrderDto>
{
    public async Task<Result<GarbageOrderDto>> HandleAsync(
        GarbageOrderPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var garbageOrder = await context.GarbageOrders
            .Include(x => x.GarbageOrderUsers)
            .FirstOrDefaultAsync(
                x => x.GarbageGroupId == request.GarbageGroupId && x.Id == request.GarbageOrderId,
                cancellationToken);

        if (garbageOrder is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotFound, HttpStatusCode.BadRequest);
        }

        var garbageOrderUser = garbageOrder.GarbageOrderUsers
            .FirstOrDefault(x => x.UserId == request.CurrentUserId);

        if (garbageOrderUser is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.Forbidden, HttpStatusCode.Forbidden);
        }

        if (garbageOrderUser.HasAcceptedPayment)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.PaymentAlreadyCompleted, HttpStatusCode.BadRequest);
        }

        if (garbageOrder.GarbageOrderUsers.Count == 0)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        if (garbageOrder.Cost < 0)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var wallet = await context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == request.CurrentUserId, cancellationToken);

        if (wallet is null)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        var amountToCharge = garbageOrderUser.ShareAmount;

        if (amountToCharge < 0)
        {
            return Result<GarbageOrderDto>.Failure(ApiErrorCodes.GenericError, HttpStatusCode.BadRequest);
        }

        if (amountToCharge > 0)
        {
            var amountToChargeDouble = decimal.ToDouble(amountToCharge);

            if (wallet.Funds < amountToChargeDouble)
            {
                return Result<GarbageOrderDto>.Failure(ApiErrorCodes.NotEnoughFunds, HttpStatusCode.BadRequest);
            }

            wallet.Funds -= amountToChargeDouble;

            context.WalletTransactions.Add(new WalletTransaction
            {
                Id = Guid.CreateVersion7(),
                WalletId = wallet.Id,
                Amount = amountToChargeDouble,
                TransactionType = TransactionType.GarbageExpense
            });
        }

        garbageOrderUser.HasAcceptedPayment = true;

        var anyPendingPayment = garbageOrder.GarbageOrderUsers.Any(x => !x.HasAcceptedPayment);

        if (!anyPendingPayment)
        {
            garbageOrder.GarbageOrderStatus = GarbageOrderStatus.WaitingForAccept;
        }
        else if (garbageOrder.GarbageOrderStatus == GarbageOrderStatus.Created)
        {
            garbageOrder.GarbageOrderStatus = GarbageOrderStatus.WaitingForPayment;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<GarbageOrderDto>.Success(garbageOrder.MapToGarbageOrderDto());
    }
}
