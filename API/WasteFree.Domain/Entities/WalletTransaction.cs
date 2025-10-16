using WasteFree.Shared.Enums;
using WasteFree.Shared.Models;

namespace WasteFree.Shared.Entities;

public class WalletTransaction : DatabaseEntity
{
    public double Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
}