using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Application.Features.GarbageOrders.Dtos;

public class GarbageOrderDto
{
    public Guid Id { get; set; }
    public PickupOption PickupOption { get; set; }
    
    public ContainerSize? ContainerSize { get; set; }
    public DateTime? DropOffDate { get; set; }
    
    public DateTime PickupDate { get; set; }

    public bool IsHighPriority { get; set; } = false;
    public bool CollectingService { get; set; } = false;
    
    public GarbageOrderStatus GarbageOrderStatus { get; set; }

    public decimal Cost { get; set; }

    public Guid GarbageGroupId { get; set; }
    public string GarbageGroupName { get; set; } = string.Empty;
    public bool GarbageGroupIsPrivate { get; set; }

    public Guid? AssignedGarbageAdminId { get; set; }
    public string? AssignedGarbageAdminUsername { get; set; }
    public ICollection<GarbageOrderUserDto> Users { get; set; } = [];
}

public static class GarbageOrderDtoExtensions
{
    public static GarbageOrderDto MapToGarbageOrderDto(this GarbageOrder garbageOrder)
    {
        return new GarbageOrderDto
        {
            Id = garbageOrder.Id,
            PickupOption = garbageOrder.PickupOption,
            ContainerSize = garbageOrder.ContainerSize,
            DropOffDate = garbageOrder.DropOffDate,
            PickupDate = garbageOrder.PickupDate,
            IsHighPriority = garbageOrder.IsHighPriority,
            CollectingService = garbageOrder.CollectingService,
            GarbageOrderStatus = garbageOrder.GarbageOrderStatus,
            Cost = garbageOrder.Cost,
            GarbageGroupId = garbageOrder.GarbageGroupId,
            GarbageGroupName = garbageOrder.GarbageGroup?.Name ?? string.Empty,
            GarbageGroupIsPrivate = garbageOrder.GarbageGroup?.IsPrivate ?? false,
            AssignedGarbageAdminId = garbageOrder.AssignedGarbageAdminId,
            AssignedGarbageAdminUsername = garbageOrder.AssignedGarbageAdmin?.Username,
            Users = garbageOrder.GarbageOrderUsers.Select(x => new GarbageOrderUserDto
            {
                UserId = x.UserId,
                Username = x.User?.Username ?? string.Empty,
                HasAcceptedPayment = x.HasAcceptedPayment,
                ShareAmount = x.ShareAmount
            }).ToList()
        };
    }
}
