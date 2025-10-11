using Microsoft.EntityFrameworkCore;
using WasteFree.Shared.Entities;
using WasteFree.Shared.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class WalletTransactionSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var plannedTransactions = new[]
        {
            new { Username = "test1", Amount = 150.0, Type = TransactionType.Deposit },
            new { Username = "test1", Amount = -40.0, Type = TransactionType.GarbageExpense },
            new { Username = "test2", Amount = 200.0, Type = TransactionType.Deposit },
            new { Username = "test2", Amount = -20.0, Type = TransactionType.GarbageExpense },
            new { Username = "test4", Amount = -15.0, Type = TransactionType.Withdrawal },
            new { Username = "garbageadmin1", Amount = 500.0, Type = TransactionType.Deposit }
        };

        var usernames = plannedTransactions.Select(t => t.Username).Distinct().ToArray();
        var walletLookup = await context.Wallets
            .Include(w => w.User)
            .Where(w => usernames.Contains(w.User.Username))
            .ToDictionaryAsync(w => w.User.Username, w => w.Id);

        var changesMade = false;
        foreach (var transaction in plannedTransactions)
        {
            if (!walletLookup.TryGetValue(transaction.Username, out var walletId))
            {
                continue;
            }

            var alreadySeeded = await context.WalletTransactions.AnyAsync(t =>
                t.WalletId == walletId && Math.Abs(t.Amount - transaction.Amount) < 0.0001 && t.TransactionType == transaction.Type);

            if (alreadySeeded)
            {
                continue;
            }

            await context.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = walletId,
                Amount = transaction.Amount,
                TransactionType = transaction.Type
            });
            changesMade = true;
        }

        if (changesMade)
        {
            await context.SaveChangesAsync();
        }
    }
}
