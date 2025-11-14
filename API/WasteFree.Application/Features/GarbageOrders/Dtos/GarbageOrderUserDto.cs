namespace WasteFree.Application.Features.GarbageOrders.Dtos;

public class GarbageOrderUserDto
{
    public Guid UserId { get; set; }
    public required string Username { get; set; }
    public bool HasAcceptedPayment { get; set; }
    public decimal ShareAmount { get; set; }
    public decimal AdditionalUtilizationFeeShareAmount { get; set; }
}
