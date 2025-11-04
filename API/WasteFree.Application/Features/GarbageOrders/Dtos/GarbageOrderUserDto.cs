namespace WasteFree.Application.Features.GarbageOrders.Dtos;

public class GarbageOrderUserDto
{
    public Guid UserId { get; set; }
    public bool HasAcceptedPayment { get; set; }
}
