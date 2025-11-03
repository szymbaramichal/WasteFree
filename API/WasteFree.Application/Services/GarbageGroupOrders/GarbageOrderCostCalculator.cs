using WasteFree.Domain.Enums;

namespace WasteFree.Application.Services.GarbageGroupOrders;

public interface IGarbageOrderCostCalculator
{
    decimal CalculateEstimate(
        PickupOption pickupOption,
        ContainerSize? containerSize,
        DateTime? dropOffDate,
        DateTime pickupDate,
        bool isHighPriority,
        bool collectingService);
}

public class GarbageOrderCostCalculator : IGarbageOrderCostCalculator
{
    private const decimal SmallPickupBase = 50m;
    private const decimal PickupBase = 80m;
    private const decimal ContainerBase = 120m;
    private const decimal SpecialOrderBase = 200m;
    private const decimal CollectingServiceFee = 35m;
    private const decimal HighPriorityMultiplier = 1.25m;
    private const decimal ContainerDailyRate = 15m;

    public decimal CalculateEstimate(
        PickupOption pickupOption,
        ContainerSize? containerSize,
        DateTime? dropOffDate,
        DateTime pickupDate,
        bool isHighPriority,
        bool collectingService)
    {
        var estimate = pickupOption switch
        {
            PickupOption.SmallPickup => SmallPickupBase,
            PickupOption.Pickup => PickupBase,
            PickupOption.Container => ContainerBase + ResolveContainerSizeFee(containerSize),
            PickupOption.SpecialOrder => SpecialOrderBase,
            _ => SmallPickupBase
        };

        if (pickupOption == PickupOption.Container)
        {
            var rentalDays = CalculateRentalDays(dropOffDate, pickupDate);
            estimate += rentalDays * ContainerDailyRate;
        }

        if (collectingService)
        {
            estimate += CollectingServiceFee;
        }

        if (isHighPriority)
        {
            estimate *= HighPriorityMultiplier;
        }

        return Math.Round(estimate, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal ResolveContainerSizeFee(ContainerSize? containerSize) => containerSize switch
    {
        ContainerSize.ContainerSmall => 40m,
        ContainerSize.ContainerMedium => 60m,
        ContainerSize.ContainerLarge => 90m,
        _ => 0m
    };

    private static int CalculateRentalDays(DateTime? dropOffDate, DateTime pickupDate)
    {
        if (!dropOffDate.HasValue)
        {
            return 0;
        }

        var duration = (pickupDate.Date - dropOffDate.Value.Date).Days;
        return Math.Max(duration, 0);
    }
}
