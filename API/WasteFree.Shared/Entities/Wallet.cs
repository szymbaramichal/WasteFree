using WasteFree.Shared.Models;

namespace WasteFree.Shared.Entities;

public class Wallet : DatabaseEntity
{
    public double Funds { get; set; }

    public string? WithdrawalAccountNumber { get; set; }
                       
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = [];
}