using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class WalletTransaction : DatabaseEntity
{
    public double Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
}