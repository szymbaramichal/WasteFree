using WasteFree.Shared.Enums;

namespace WasteFree.Shared.Constants;

public static class WalletMethods
{
    private static readonly IReadOnlyCollection<WalletMethod> availableMethods = new List<WalletMethod>
    {
        new("BLIK", "Blik", nameof(TransactionType.Deposit)),
        new("IBAN", "Bank Withdrawal", nameof(TransactionType.Withdrawal))
    };
    
    public static IReadOnlyCollection<WalletMethod> AvailableMethods => availableMethods;
}

public record WalletMethod(string Code, string Name, string Type);