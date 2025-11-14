namespace WasteFree.Domain.Enums;

/// <summary>
/// Types of financial transactions recorded in the system.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Money deposited to the wallet.
    /// </summary>
    Deposit = 0,

    /// <summary>
    /// Money withdrawn from the wallet.
    /// </summary>
    Withdrawal = 1,

    /// <summary>
    /// Expense related to garbage services.
    /// </summary>
    GarbageExpense = 2,

    /// <summary>
    /// Refund transaction returning funds to the user.
    /// </summary>
    Refund = 3,

    /// <summary>
    /// Income received by a garbage admin after completing an order.
    /// </summary>
    GarbageIncome = 4
}