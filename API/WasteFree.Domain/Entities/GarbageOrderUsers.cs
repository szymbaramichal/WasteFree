using WasteFree.Domain.Models;

namespace WasteFree.Domain.Entities;

public class GarbageOrderUsers : DatabaseEntity
{
    public Guid GarbageOrderId { get; set; }
    public GarbageOrder GarbageOrder { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool HasAcceptedPayment { get; set; } = false;

    public decimal ShareAmount { get; set; }
    public decimal AdditionalUtilizationFeeShareAmount { get; set; }
}