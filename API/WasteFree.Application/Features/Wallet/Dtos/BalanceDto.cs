namespace WasteFree.Business.Features.Wallet.Dtos;

/// <summary>
/// DTO returned for wallet balance queries.
/// </summary>
public record BalanceDto
{
    /// <summary>
    /// Current balance amount available in the wallet.
    /// </summary>
    public double Amount { get; init; }
}
