namespace WasteFree.Application.Features.GarbageGroupOrders.Dtos;

public class GarbageOrderUserDto
{
    public Guid UserId { get; set; }
    public bool HasAcceptedPayment { get; set; }
}