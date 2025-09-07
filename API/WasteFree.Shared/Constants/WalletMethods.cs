using WasteFree.Shared.Enums;

namespace WasteFree.Shared.Constants;

public static class WalletMethods
{
    private static readonly IReadOnlyCollection<WalletMethod> availableMethods = new List<WalletMethod>
    {
        new("BLIK", "Blik", TransactionType.Deposit),
        new("IBAN", "Bank Withdrawal", TransactionType.Withdrawal)
    };
    
    public static IReadOnlyCollection<WalletMethod> AvailableMethods => availableMethods;
}

public record WalletMethod(string Code, string Name, TransactionType Type);