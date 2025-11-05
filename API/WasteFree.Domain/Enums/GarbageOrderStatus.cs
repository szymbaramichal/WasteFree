namespace WasteFree.Domain.Enums;


/// <summary>
/// Represents the status of a garbage order.
/// </summary>
public enum GarbageOrderStatus
{
    /// <summary>
    /// Waiting for payment to be processed.
    /// </summary>
    WaitingForPayment,
    /// <summary>
    /// Waiting for the order to be accepted.
    /// </summary>
    WaitingForAccept,
    /// <summary>
    /// Waiting for pickup of the garbage.
    /// </summary>
    WaitingForPickup,
    /// <summary>
    /// Waiting for the utilization fee to be paid.
    /// </summary>
    WaitingForUtilizationFee,
    /// <summary>
    /// The order has been completed.
    /// </summary>
    Completed,
    /// <summary>
    /// A complaint has been filed regarding the order.
    /// </summary>
    Complained,
    /// <summary>
    /// The complaint or issue has been resolved.
    /// </summary>
    Resolved,
    /// <summary>
    /// The order has been cancelled because required payments were not completed.
    /// </summary>
    Cancelled
}