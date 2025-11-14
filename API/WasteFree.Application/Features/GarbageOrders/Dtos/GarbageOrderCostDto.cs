namespace WasteFree.Application.Features.GarbageOrders.Dtos;

/// <summary>
/// Represents the calculated garbage order cost components returned to clients.
/// </summary>
public class GarbageOrderCostDto
{
    /// <summary>
    /// Estimated service cost before applying the prepaid utilization fee.
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Prepaid utilization fee amount added to the base estimate.
    /// </summary>
    public decimal PrepaidUtilizationFee { get; set; }

    /// <summary>
    /// Total estimated amount including the prepaid utilization fee.
    /// </summary>
    public decimal EstimatedTotalCost { get; set; }
}
