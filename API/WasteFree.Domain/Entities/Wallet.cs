using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class Wallet : DatabaseEntity
{
    public double Funds { get; set; }

    public string? WithdrawalAccountNumber { get; set; }
                       
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = [];
}