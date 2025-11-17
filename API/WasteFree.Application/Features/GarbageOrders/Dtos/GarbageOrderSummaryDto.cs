using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Application.Features.GarbageOrders.Dtos;

public class GarbageOrderSummaryDto
{
    public Guid Id { get; set; }
    public PickupOption PickupOption { get; set; }
    public ContainerSize? ContainerSize { get; set; }
    public DateTime? DropOffDate { get; set; }
    public DateTime PickupDate { get; set; }
    public bool IsHighPriority { get; set; }
    public bool CollectingService { get; set; }
    public GarbageOrderStatus GarbageOrderStatus { get; set; }
    public decimal Cost { get; set; }
    public Guid GarbageGroupId { get; set; }
    public string GarbageGroupName { get; set; } = string.Empty;
    public bool GarbageGroupIsPrivate { get; set; }
    public Guid? AssignedGarbageAdminId { get; set; }
    public string? AssignedGarbageAdminUsername { get; set; }
    public double? DistanceInKilometers { get; set; }
}

public static class GarbageOrderSummaryDtoExtensions
{
    public static GarbageOrderSummaryDto MapToGarbageOrderSummaryDto(
        this GarbageOrder garbageOrder,
        double? distanceInKilometers = null)
    {

        return new GarbageOrderSummaryDto
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
            DistanceInKilometers = distanceInKilometers
        };
    }
}
