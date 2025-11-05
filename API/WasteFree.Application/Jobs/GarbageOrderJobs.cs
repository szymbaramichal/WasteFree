using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Models;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Jobs;

public class GarbageOrderJobs(ApplicationDataContext context, ILogger<GarbageOrderJobs> logger)
{
    private readonly ApplicationDataContext _context = context;
    private readonly ILogger<GarbageOrderJobs> _logger = logger;

    [TickerFunction(functionName: nameof(CancelUnpaidGarbageOrders), cronExpression: "0 * * * *")]
    public async Task CancelUnpaidGarbageOrders(TickerFunctionContext<string> tickerContext, CancellationToken cancellationToken)
    {
        var cutoffUtc = DateTime.UtcNow.AddHours(-1);

        var candidateOrders = await _context.GarbageOrders
            .Include(o => o.GarbageOrderUsers)
            .Where(o => o.CreatedDateUtc <= cutoffUtc)
            .Where(o => o.GarbageOrderStatus == GarbageOrderStatus.WaitingForPayment)
            .Where(o => o.GarbageOrderUsers.Any(u => !u.HasAcceptedPayment))
            .ToListAsync(cancellationToken);

        if (candidateOrders.Count == 0)
        {
            return;
        }

        var refundableUsers = candidateOrders
            .SelectMany(o => o.GarbageOrderUsers)
            .Where(u => u.HasAcceptedPayment && u.ShareAmount > 0)
            .Select(u => u.UserId)
            .Distinct()
            .ToList();

        var wallets = await _context.Wallets
            .Where(w => refundableUsers.Contains(w.UserId))
            .ToDictionaryAsync(w => w.UserId, cancellationToken);

        foreach (var order in candidateOrders)
        {
            foreach (var user in order.GarbageOrderUsers.Where(u => u.HasAcceptedPayment && u.ShareAmount > 0))
            {
                if (!wallets.TryGetValue(user.UserId, out var wallet))
                {
                    _logger.LogWarning("Wallet not found for user {UserId} while cancelling order {OrderId}", user.UserId, order.Id);
                    continue;
                }

                var refundAmount = (double)user.ShareAmount;
                wallet.Funds += refundAmount;

                _context.WalletTransactions.Add(new WalletTransaction
                {
                    Id = Guid.CreateVersion7(),
                    WalletId = wallet.Id,
                    Amount = refundAmount,
                    TransactionType = TransactionType.Refund
                });

                user.HasAcceptedPayment = false;
            }

            order.GarbageOrderStatus = GarbageOrderStatus.Cancelled;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled {Count} garbage orders that did not receive all payments", candidateOrders.Count);
    }
}
