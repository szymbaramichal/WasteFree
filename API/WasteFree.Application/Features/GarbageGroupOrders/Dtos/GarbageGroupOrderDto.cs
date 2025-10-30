using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Application.Features.GarbageGroupOrders.Dtos;

public class GarbageGroupOrderDto
{
    public Guid Id { get; set; }
    public PickupOption PickupOption { get; set; }
    
    public ContainerSize? ContainerSize { get; set; }
    public DateTime? DropOffDate { get; set; }
    
    public DateTime PickupDate { get; set; }

    public bool IsHighPriority { get; set; } = false;
    public bool CollectingService { get; set; } = false;
    
    public GarbageOrderStatus GarbageOrderStatus { get; set; }

    public Guid GarbageGroupId { get; set; }
    public ICollection<Guid> Users { get; set; } = [];
}

public static class GarbageGroupOrderDtoExtensions
{
    public static GarbageGroupOrderDto MapToGarbageGroupOrderDto(this GarbageOrder garbageOrder)
    {
        return new GarbageGroupOrderDto
        {
            Id = garbageOrder.Id,
            PickupOption = garbageOrder.PickupOption,
            ContainerSize = garbageOrder.ContainerSize,
            DropOffDate = garbageOrder.DropOffDate,
            PickupDate = garbageOrder.PickupDate,
            IsHighPriority = garbageOrder.IsHighPriority,
            CollectingService = garbageOrder.CollectingService,
            GarbageOrderStatus = garbageOrder.GarbageOrderStatus,
            GarbageGroupId = garbageOrder.GarbageGroupId,
            Users = garbageOrder.GarbageOrderUsers.Select(x => x.UserId).ToList()
        };
    }
}
