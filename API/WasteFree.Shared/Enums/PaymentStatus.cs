namespace WasteFree.Shared.Enums;

/// <summary>
/// Indicates the current status of a payment transaction.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The payment is invalid or not recognized.
    /// </summary>
    Invalid,

    /// <summary>
    /// The payment is pending and not yet completed.
    /// </summary>
    Pending,

    /// <summary>
    /// The payment has been completed successfully.
    /// </summary>
    Completed
}